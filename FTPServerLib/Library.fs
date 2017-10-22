namespace FTPServerLib
open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading
open FTPCommands
open DirectoryHelpers

module ServerConfiguration = 
    let commandPort = 2121
    let dataPort = 2122
    let localHost = "127.0.0.1"

// All socket and conneection related stuff.
module ServerHelpers =
    open ServerConfiguration

    let createSocket port toListen =
        let commandLocalEndPoint = IPEndPoint(IPAddress.Parse(localHost), port)  
        let socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        
        match toListen with
        | true ->  
            socket.Bind(commandLocalEndPoint)
            socket.Listen(111)  
        | false -> socket.Connect(commandLocalEndPoint)
        
        socket

    let createCommandSocket = createSocket commandPort
    
    let createDataSocket = createSocket dataPort
    
    let readFromStream (stream:NetworkStream) =
        let buffer: byte [] = Array.zeroCreate 1024
        let asciiBuffer = System.Text.Encoding.ASCII.GetString(buffer).ToCharArray() 
        let ftpCommand = 
            let charArray: char array = asciiBuffer |> Seq.takeWhile (fun c -> c <> '\r') |> Seq.toArray
            String charArray
        //printfn "Received Command : %s " ftpCommand
        ftpCommand
    
    let writeToSocket(socket:Socket) (data:byte array) =
        socket.Send (data) 
    
    let writeToFile file (dataToWrite:string) =
        let fullPath = Path.Combine(currentDirectory(), file) 
        use fs = File.Create(fullPath)
        let dataInBytes = System.Text.Encoding.ASCII.GetBytes(dataToWrite); 
        fs.Write(dataInBytes, 0, dataInBytes.Length);

    let streamToFile file = readFromStream >> writeToFile file

    let writeBytesToStream (stream:NetworkStream) (data:byte array) =
        stream.Write(data, 0, data.Length)
        stream.Flush()

    let writeToStream (stream:NetworkStream) endOfMessage (data:string) =
        let dataToWrite = if endOfMessage then data+"\r" else data 
        let msgInBytes = System.Text.Encoding.ASCII.GetBytes(dataToWrite);  
        writeBytesToStream stream msgInBytes
        
    let readCommand =
        readFromStream >> parseFTPCommand
    
    let RespondWithServerCode  (stream:NetworkStream) code =
        getServerReturnMessageWithCode code 
        |> writeToStream stream true

 module CommandResponse =
    open ServerHelpers

    let writeFileToClient fileName =
        let data = getFileContent fileName
        System.Threading.Thread.Sleep 3000
        let dataSendingSocket = createDataSocket false
        let stream = new NetworkStream(dataSendingSocket, false) 
        writeToStream stream true data

    let readFileFromClient fileName =
        let data = "" // todo : Implement reading from the client data socket
        writeToFile fileName data

    let handleCommand stream cmd =
        match cmd with
        | USER user -> failwithf "shouldn't call USER command with args:[%s]" user
        | PASS pass -> failwithf "shouldn't call PASS command with args:[%s]" pass
        | HELP -> writeToStream stream true "Help just google it dude!"
        | CLOSE -> RespondWithServerCode stream ServerReturnCodeEnum.ClosingControlConnection
        | PWD -> currentDirectory() |> sprintf "Current dir is : %s " |> writeToStream stream true 
        | CD newPath -> changeCurrentDirectory newPath    
                        writeToStream stream false "Directory got changed!.\n"
                        RespondWithServerCode stream ServerReturnCodeEnum.Successfull
        | LIST ->  getResponseToDir() |> writeToStream stream true 
        | RETR file -> RespondWithServerCode stream ServerReturnCodeEnum.Successfull 
                       writeFileToClient file
        | STOR file -> RespondWithServerCode stream ServerReturnCodeEnum.Successfull 
                       readFileFromClient file
        | UNSUPPORTED -> writeToStream stream true "Unsupported command!"
    
module UserSession =
    open ServerHelpers
    open CommandResponse
    /// this is ran after the user successfully logged in
    let startUserSession userName (stream:NetworkStream) =
        // parse commands
        // do stuff
        let rec readAndParseCommand () =
            let mutable port = None        // ---> PORT 192,168,150,80,14,178
            let cmd = readCommand stream
            handleCommand stream cmd
            match cmd with
            | CLOSE -> exit 0                   
            | _ -> readAndParseCommand ()

        readAndParseCommand ()
    
    let handleUserLogin (userName:string, stream:NetworkStream) = 
        sprintf "Welocome User,  %s !\n" userName |>  writeToStream stream false
        // ---> USER slacker
        // 331 Password required for slacker.
        RespondWithServerCode stream ServerReturnCodeEnum.PasswordRequest
        let command = readCommand stream
        match command with
            | PASS passwd -> 
                if(passwd = "test") then 
                    RespondWithServerCode stream ServerReturnCodeEnum.Successfull
                    startUserSession userName stream 
                else 
                    RespondWithServerCode stream ServerReturnCodeEnum.InvalidCredential
            | _ -> RespondWithServerCode stream ServerReturnCodeEnum.PasswordRequest
    
    let createSession (socket:Socket) =
        async {
            let stream = new NetworkStream(socket, false) 
            writeToStream stream false "Connected to FTP server by F#! \n"  
            //RespondWithServerCode stream ServerReturnCodeEnum.FTPServeReady
            while true do
                let command = readCommand stream
                match command with
                | USER userName -> handleUserLogin (userName, stream)
                | _ -> writeToStream stream true "Login with USER command!.\n"  
    
            stream.Close()
            socket.Shutdown(SocketShutdown.Both)
            socket.Close()
        }

module Main =
    open UserSession
    open ServerHelpers
    let StartServer() =
        let socket = createCommandSocket true
        printfn "Waiting for request ..."
        
        let connectionLimit = 10
        let connectionCount = ref 0
        
        while !connectionCount < connectionLimit do
            let socket1 = socket.Accept()
            incr connectionCount    // increase value of connectionCount by 1
            
            let cancellationSource = new CancellationTokenSource()
            let sessionAsync = createSession socket1
                               
            Async.StartWithContinuations (sessionAsync, (fun () -> decr connectionCount), // decrease by 1
                                                        (fun (x:exn) -> printfn "%s \n%s" x.Message x.StackTrace
                                                                        decr connectionCount),
                                                        (fun (x:OperationCanceledException) -> printfn "session cancelled"
                                                                                               decr connectionCount)) 
        printfn "Finally finished!"