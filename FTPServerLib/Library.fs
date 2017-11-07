namespace FTPServerLib
open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading
open FTPCommands
open DirectoryHelpers
open FTPBasic

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
        let readLen = stream.Read(buffer, 0, 1024)
        // Fix issue while 
        let asciiBuffer = System.Text.Encoding.ASCII.GetString(buffer).ToCharArray() 
        let ftpCommand = 
            let charArray: char array = asciiBuffer |> Seq.takeWhile (fun c -> c <> '\r') |> Seq.toArray
            String charArray
        //printfn "Received Command : %s " ftpCommand
        ftpCommand
    
    let writeToSocket(socket:Socket) (data:byte array) =
        socket.Send (data) 
    
    let writeToFile file (dataToWrite:string) =
        let fullPath = Path.Combine(Directory.GetCurrentDirectory(), file) 
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
    open SessionInfo

    let writeFileToClient fileName (sessionData : SessionData) =
        let data = getFileContent fileName sessionData
        System.Threading.Thread.Sleep 3000
        let dataSendingSocket = createDataSocket false
        let stream = new NetworkStream(dataSendingSocket, false) 
        writeToStream stream true data

    let readFileFromClient fileName =
        let dataSendingSocket = createDataSocket false
        let stream = new NetworkStream(dataSendingSocket, false) 
        let data = readFromStream stream 
        writeToFile fileName data

    //----Command handling functions----
    let handleUser (sessionData : SessionData) user =
        failwithf "shouldn't call USER command with args:[%s]" user
        sessionData
    let handlePass (sessionData : SessionData) pass =
        failwithf "shouldn't call PASS command with args:[%s]" pass
        sessionData
    
    let handleHelp (sessionData : SessionData) stream =
         writeToStream stream true "Help just google it dude!"
         sessionData

    let handleClose (sessionData : SessionData) stream =
        RespondWithServerCode stream ServerReturnCodeEnum.ClosingControlConnection
        sessionData

    let handlePwd sessionData stream =
        sessionData.CurrentPath 
        |> sprintf "Current dir is : %s " 
        |> writeToStream stream true 
        sessionData 
    
    let handleCd sessionData stream newPath =
        writeToStream stream false "Directory got changed!.\n"
        RespondWithServerCode stream ServerReturnCodeEnum.Successfull
        updateCurrentPath sessionData newPath

    let handleList sessionData stream =
        getResponseToDir sessionData |> writeToStream stream true 
        sessionData

    let handleRetr sessionData stream file = 
        RespondWithServerCode stream ServerReturnCodeEnum.Successfull 
        writeFileToClient file sessionData
        sessionData
    
    let handleStor (sessionData : SessionData) stream file  = 
        RespondWithServerCode stream ServerReturnCodeEnum.Successfull 
        readFileFromClient file
        sessionData

    let handlePort sessionData stream port =
        RespondWithServerCode stream ServerReturnCodeEnum.Successfull 
        writeToStream stream false "Data Port got registered!.\n"
        updatePort sessionData port
    
    let handleMode sessionData stream cmd =
        RespondWithServerCode stream ServerReturnCodeEnum.Successfull
        writeToStream stream false "Mode got changed!.\n"
        match cmd with
        | PASSIVE  -> 
            updateMode sessionData true
        | _ -> updateMode sessionData false

    let handleUnsupported (sessionData : SessionData) stream =
        writeToStream stream true "Unsupported command!"
        sessionData  
    
    //---------------End----------------
    let handleCommand (sessionData : SessionData) stream cmd =
        let updatedSessionData = updateCmdHistory sessionData cmd
        // Addling differente helpor method 
        match cmd with
        | USER user -> handleUser updatedSessionData user
        | PASS pass -> handlePass updatedSessionData pass
        | HELP -> handleHelp updatedSessionData stream
        | CLOSE -> handleClose updatedSessionData stream
        | PWD -> handlePwd updatedSessionData stream
        | CD newPath -> handleCd updatedSessionData stream newPath
        | LIST ->  handleList updatedSessionData stream
        | RETR file -> handleRetr updatedSessionData stream file
        | STOR file -> handleStor updatedSessionData stream file
        | PORT port -> handlePort updatedSessionData stream <| Some(port)
        | PASSIVE | ACTIVE -> handleMode updatedSessionData stream cmd
        | UNSUPPORTED -> handleUnsupported updatedSessionData stream

module UserSession =
    open ServerHelpers
    open CommandResponse
    open SessionInfo
    
    /// this is ran after the user successfully logged in
    let startUserSession sessionData stream =
        // parse commands do stuff
        let rec readAndParseCommand sessionData : SessionData =
            match readCommand stream with
            | CLOSE -> handleCommand sessionData stream CLOSE                
            | cmd ->
                handleCommand sessionData stream cmd
                |> readAndParseCommand
        readAndParseCommand sessionData 
    
    let handleUserLogin userName sessionData stream = 
        let sessionDataWithName = updateUserName sessionData userName
        sprintf "Welocome User,  %s !\n" userName |>  writeToStream stream false

        RespondWithServerCode stream ServerReturnCodeEnum.PasswordRequest

        match userName.ToUpper() with 
        | "ANONYMOUS" -> 
            RespondWithServerCode stream ServerReturnCodeEnum.Successfull
            startUserSession sessionData stream |> ignore
        | _ ->
            let command = readCommand stream
            let updatedSessionData = updateCmdHistory sessionDataWithName command
            match command with
            | PASS passwd -> 
                if(passwd = "test") then 
                    RespondWithServerCode stream ServerReturnCodeEnum.Successfull
                    startUserSession updatedSessionData stream |> ignore
                else 
                    RespondWithServerCode stream ServerReturnCodeEnum.InvalidCredential
            | _ -> RespondWithServerCode stream ServerReturnCodeEnum.PasswordRequest

    let createSession socket =
        async {
            let nStream = new NetworkStream(socket, false) 
            let sessionData = 
                {
                    CmdHistory = List.Empty
                    CurrentPath = Directory.GetCurrentDirectory()
                    UserName = ""
                    Port = None
                    PassiveModeOn = false
                }
            writeToStream nStream false "Connected to FTP server by F#! \n"  
            //RespondWithServerCode stream ServerReturnCodeEnum.FTPServeReady
            while true do
                let command = readCommand nStream
                let updatedSessionData = updateCmdHistory sessionData command
                match command with
                | USER userName -> handleUserLogin userName updatedSessionData nStream
                | _ -> writeToStream nStream true "Login with USER command!.\n"  
    
            nStream.Close()
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
            printfn "Connection %d started." connectionCount.Value
            let cancellationSource = new CancellationTokenSource()
            let sessionAsync = createSession socket1
                               
            Async.StartWithContinuations (sessionAsync, (fun () -> decr connectionCount), // decrease by 1
                                                        (fun (x:exn) -> printfn "%s \n%s" x.Message x.StackTrace
                                                                        decr connectionCount),
                                                        (fun (x:OperationCanceledException) -> printfn "session cancelled"
                                                                                               decr connectionCount)) 
            printfn "Connection %d ended." connectionCount.Value
        printfn "Finally finished!"