namespace FTPServerLib
open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading
open FTPCommands
open DirectoryHelpers

// All socket and conneection related stuff.
module ServerHelpers =
    let readFromStream (stream:NetworkStream) =
        let buffer: byte [] = Array.zeroCreate 1024
        let readLen = stream.Read(buffer, 0, 1024)
        
        let asciiBuffer = System.Text.Encoding.ASCII.GetString(buffer).ToCharArray() 
        let ftpCommand = 
            let charArray: char array = asciiBuffer |> Seq.takeWhile (fun c -> c <> '\r') |> Seq.toArray
            System.String charArray
        //printfn "Received Command : %s " ftpCommand
        ftpCommand
    //let writeToSocket(socket:Socket) (data:byte array) =
      //  socket.Send(data, 0, data.Length) 
    let writeBytesToStream (stream:NetworkStream) (data:byte array) =
        stream.Write(data, 0, data.Length)

    let writeToStream (stream:NetworkStream) endOfMessage (data:string) =
        let dataToWrite = if endOfMessage then data+"\r" else data 
        let msgInBytes = System.Text.Encoding.ASCII.GetBytes(dataToWrite);  
        writeBytesToStream stream msgInBytes
        
    let readCommand =
        readFromStream >> parseFTPCommand
    
    let RespondWithServerCode  (stream:NetworkStream) code =
        getServerReturnMessageWithCode code 
        |> writeToStream stream true

module UserSession =
    open ServerHelpers
    /// this is ran after the user successfully logged in
    let startUserSession userName (stream:NetworkStream) =
        // parse commands
        // do stuff
        let rec readAndParseCommand () =
            let mutable port = None        // ---> PORT 192,168,150,80,14,178
            let cmd = readCommand stream
            match cmd with
            | USER user -> failwithf "shouldn't call USER command with args:[%s]" user
            | PASS pass -> failwithf "shouldn't call PASS command with args:[%s]" pass
            | HELP -> writeToStream stream true "Help just google it dude!"
            | CLOSE -> RespondWithServerCode stream ServerReturnCodeEnum.ClosingControlConnection
            | UNSUPPORTED -> writeToStream stream true "Unsupported command!"
            | LIST ->  writeToStream stream true getResponseToDir
            | _ -> writeToStream stream true "Unable to find the proper command!"
            
            match cmd with
            | CLOSE -> exit 0                   
            | _ -> readAndParseCommand ()

        readAndParseCommand ()
    
    let handleUserLogin (userName:string, stream:NetworkStream) = 
        "Welocome User : " + userName |>  writeToStream stream false
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
                | _ -> writeToStream stream true "Login with USER command!."  
    
            stream.Close()
            socket.Shutdown(SocketShutdown.Both)
            socket.Close()
        }

module Main =
    open UserSession
    let commandPort = 2121
    let localHost = "127.0.0.1"
    let StartServer() =
        let commandLocalEndPoint = IPEndPoint(IPAddress.Parse(localHost), commandPort)  
        let socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        socket.Bind(commandLocalEndPoint)
        socket.Listen(111)  
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