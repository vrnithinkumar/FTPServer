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
    let readAndWriteToStream (stream:NetworkStream) =
        let buffer: byte [] = Array.zeroCreate 1024
        let readLen = stream.Read(buffer, 0, 1024)
        let ftpCommand = System.Text.Encoding.ASCII.GetString(buffer) 
        // printfn "Received Command : %s " ftpCommand
        let responseToSend = getResponse ftpCommand
        // printfn "Response is : %s " responseToSend
    
        let commandResponse = 
          responseToSend
          |> System.Text.Encoding.ASCII.GetBytes
    
        stream.Write(commandResponse, 0, commandResponse.Length)
        
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
        let resp =  GetServerReturnMessageWithCode code 
        printfn "Resp %s" resp 
        writeToStream stream true resp

module UserSession =
    open ServerHelpers
    /// this is ran after the user successfully logged in
    let startUserSession userName (stream:NetworkStream) =
        // parse commands
        // do stuff
        let rec readAndParseCommand () =
            let mutable port = None        // ---> PORT 192,168,150,80,14,178
            //use stream = new NetworkStream(socket)
            let cmd = readCommand stream
            match cmd with
            | USER user -> failwithf "shouldn't call USER command with args:[%s]" user
            | PASS pass -> failwithf "shouldn't call PASS command with args:[%s]" pass
            | HELP -> writeToStream stream true "Help just google it dude"
            | CLOSE -> writeToStream stream true "Good bye"
            | UNSUPPORTED -> writeToStream stream true "Unsupported command"
            | _ -> writeToStream stream true "unable to find the proper command"
            match cmd with
            | CLOSE -> ()                    
            | _ -> readAndParseCommand ()
           // stream.Flush()
        readAndParseCommand ()
    
    let handleUserLogin (userName:string, stream:NetworkStream) = 
        "Welocme User : " + userName |>  writeToStream stream false
        // ---> USER slacker
        // 331 Password required for slacker.
        RespondWithServerCode stream ServerReturnCodeEnum.PasswordRequest
        startUserSession userName stream
    
    let createSession (socket:Socket) =
        let stream = new NetworkStream(socket, false) 
        writeToStream stream false "Connected to FTP server by F#! \n"  
        let result = GetServerReturnMessageWithCode ServerReturnCodeEnum.FTPServeReady
        RespondWithServerCode stream ServerReturnCodeEnum.FTPServeReady
        while true do
            let command = readCommand stream
            match command with
            | USER userName -> handleUserLogin (userName, stream)
            | _ -> writeToStream stream true "Login with USER command!."  
        (*let stream = new NetworkStream(socket) 
        while true do
            readAndWriteToStream stream*)
    
        stream.Close()
        socket.Shutdown(SocketShutdown.Both)
        socket.Close()

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
        
        while true do
            let socket1 = socket.Accept()
            //send  220 testbox2.slacksite.com FTP server ready.
            createSession socket1
        printfn "Finally finished!"