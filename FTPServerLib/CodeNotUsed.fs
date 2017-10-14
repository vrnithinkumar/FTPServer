namespace FTPServerLib

open System.IO
open System
open System.Net
open System.Net.Sockets
open System.Threading

module DirectoryHelpers =
    let pathToTest = "/Users/vr/WorkSpace/gist/" 
    let directoryDetails path = 
        let dir = DirectoryInfo(path)
        
        let files = 
            dir.GetFiles()
            |> Array.map (fun x -> x.ToString())
        
        let folders = 
            dir.GetDirectories()
            |> Array.map (fun x -> x.ToString())
        Array.append files folders
    
    let getResponseToDir =
        let filesAndFolders =
            directoryDetails pathToTest
            |> String.concat "\n"
        ". \n..\n"+filesAndFolders
    
    let getFile file =
        let file = FileInfo(file)
        file

    let getTheCurrentDirectory =
        pathToTest

open DirectoryHelpers

module FTPCommands =
    type SupportedCommands =
        | PWD
        | USER of string
        | PASS of string
        | CLOSE
        | HELP
        | DIR of string
        | UNSUPPORTED

    let parseFTPCommand command = 
        printfn "Parsing %s" command
        
        let [| cmdName; cmdArgs |] = command.Split ' '
        match cmdName.ToLower() with
        | "pwd" -> PWD
        | "user" -> let userName = cmdArgs in USER userName        // ---> USER slacker   ---> PASS XXXX   ---> PORT 192,168,150,80,14,178
        | "pass" -> let password = cmdArgs in PASS password
        | "close" -> CLOSE
        | "help" -> HELP
        | "dir" -> let directory = cmdArgs in DIR directory
        | _ -> UNSUPPORTED
 
    let getResponseByParsing commandString =
        let command = parseFTPCommand commandString
        printfn "Command %s is %A" commandString command
        match command with
            | CLOSE -> "Connections is closed"
            | DIR   -> getResponseToDir
            | SupportedCommands.PWD   -> getTheCurrentDirectory
            | SupportedCommands.HELP  -> "Supported Commands are \n ls \n login \n close \n help \n dir"
            | SupportedCommands.UNSUPPORTED  -> "Error! \n Not supported!"

    let getResponseToDir =
        let filesAndFolders =
            directoryDetails pathToTest
            |> String.concat "\n"
        ". \n..\n" + filesAndFolders
    
    let getResponse commandString =
        printfn "Creating response for %s" commandString
        let trimmedString =commandString.Trim()
        let response =
            match commandString with
                | "login" -> 
                    printfn "Matched : %s " commandString
                    "Login as anonymous user!"
                | "close" -> 
                    printfn "Matched : %s " commandString
                    "Connections is closed"
                | "dir" -> 
                    printfn "Matched : %s " commandString
                    getResponseToDir
                | "pwd" -> 
                    printfn "Matched : %s " commandString
                    getTheCurrentDirectory
                | "help" -> 
                    printfn "Matched help : %s" commandString 
                    "Supported Commands are \n ls \n login \n close \n help \n dir"
                | _ ->
                    printfn "Matched  __: %s " commandString 
                    "Error! \n Not supported!"
                    
        printfn "Resp is ##%s" response
        response

    let Test ()= 
       let commandBuffer = System.Text.Encoding.UTF8.GetBytes("help")
       let ftpCommand = System.Text.Encoding.UTF8.GetString(commandBuffer)
       let command = getResponse ftpCommand
       printfn "Command %s is %A" "help" command



open FTPCommands
open DirectoryHelpers


// All socket and conneection related stuff.
module ServerHelpers =
    let readAndWriteToStream (stream:NetworkStream) =
        let buffer: byte [] = Array.zeroCreate 1024
        let readLen = stream.Read(buffer, 0, 1024)
        let ftpCommand = System.Text.Encoding.ASCII.GetString(buffer) 
        printfn "Received Command : %s " ftpCommand
        let responseToSend = getResponse ftpCommand
        printfn "Response is : %s " responseToSend
    
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
        
        printfn "Received Command : %s " ftpCommand
        ftpCommand
        
    let writeToStream (stream:NetworkStream) (data:byte array) =
        stream.Write(data, 0, data.Length)
        
    let readCommand =
        readFromStream >> parseFTPCommand

module UserSession =
    open ServerHelpers
    
    /// this is ran after the user successfully logged in
    let startUserSession userName socket =
        // parse commands
        // do stuff
        
        let rec readAndParseCommand () =
            let mutable port = None        // ---> PORT 192,168,150,80,14,178
            
            use stream = NetworkStream(socket)
            let cmd = readCommand stream

            match cmd with
            | USER user -> failwithf "shouldn't call USER command with args:[%s]" user
            | PASS pass -> failwithf "shouldn't call PASS command with args:[%s]" pass
            | otherCommands -> // do stuff on commands 
                               ()

            
            
            readAndParseCommand ()
        
        readAndParseCommand ()
    
    let createSession socket =
        // ask user name
        // show: Name (testbox2:slacker): slacker
        
        // ---> USER slacker
        // 331 Password required for slacker.
        use stream = new NetworkStream(socket) 
        
        let command = readCommand stream
        match command with
        | USER userName -> // act on supplied name
                            
                           // start user session
                           startUserSession userName socket
        | _ -> ()
            
        
        
        
        
        (*let stream = new NetworkStream(socket) 
        while true do
            readAndWriteToStream stream*)
    
        socket.Shutdown(SocketShutdown.Both)
        socket.Close()


module Main =
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
            
            //createUserSession socket1
            
            printfn "Finally finished!"


module SocketServer =

    open System.Net
    open System.Net.Sockets
    open System.Collections.Generic

    let toIList<'T> (data : 'T array) =
        let segment = new System.ArraySegment<'T>(data)
        let data = new List<System.ArraySegment<'T>>() :> IList<System.ArraySegment<'T>>
        data.Add(segment)
        data

    type Socket with
        member this.MyAcceptAsync() =
            Async.FromBeginEnd((fun (callback, state) -> this.BeginAccept(callback, state)),
                               this.EndAccept)
        member this.MyConnectAsync(ipAddress : IPAddress, port : int) =
            Async.FromBeginEnd(ipAddress, port,
                               (fun (ipAddress:IPAddress, port, callback, state) ->
                                   this.BeginConnect(ipAddress, port, callback, state)),
                               this.EndConnect)
        member this.MySendAsync(data : byte array, flags : SocketFlags) =
            Async.FromBeginEnd(toIList data, flags, 
                               (fun (data : IList<System.ArraySegment<byte>>,
                                     flags : SocketFlags, callback, state) ->
                                         this.BeginSend(data, flags, callback, state)),
                               this.EndSend)
        member this.MyReceiveAsync(data : byte array, flags : SocketFlags) =
            Async.FromBeginEnd(toIList data, flags, 
                               (fun (data : IList<System.ArraySegment<byte>>,
                                     flags : SocketFlags, callback, state) ->
                                         this.BeginReceive(data, flags, callback, state)),
                               this.EndReceive)

    let port = 11000

    let socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    let ipHostInfo = Dns.Resolve(Dns.GetHostName())
    let localIPAddress = ipHostInfo.AddressList.[0]
    let localEndPoint = new IPEndPoint(localIPAddress, port)
    socket.Bind(localEndPoint)


    let connectSendReceive (socket : Socket) =
        async {
            do! socket.MyConnectAsync(ipHostInfo.AddressList.[0], 11000)
            let buffer1 = [| 0uy .. 255uy |]
            let buffer2 = Array.zeroCreate<byte> 255
            let flags = new SocketFlags()
            let! flag = socket.MySendAsync(buffer1, flags)
            let! result = socket.MyReceiveAsync(buffer2, flags)
            return buffer2
        }

    let acceptReceiveSend (socket : Socket) =
        async {
            printfn "Listening..."
            socket.Listen(10)
            printfn "Accepting..."
            let! socket = socket.MyAcceptAsync()
            
            let buffer1 = Array.zeroCreate<byte> 256
            let flags = new SocketFlags()
            printfn "Receiving..."
            let! nBytes = socket.MyReceiveAsync(buffer1, flags)
            printfn "Received %d bytes from client computer." nBytes
            let buffer2 = Array.rev buffer1
            printfn "Sending..."
            let! flag = socket.MySendAsync(buffer2, flags)
            printfn "Completed."
            return buffer2
        }

    let taskServer = Async.StartAsTask(acceptReceiveSend(socket))    
    taskServer.Wait()
    socket.Close()

module SocketClient =

    open System.Net
    open System.Net.Sockets
    open System.Collections.Generic

    let toIList<'T> (data : 'T array) =
        let segment = new System.ArraySegment<'T>(data)
        let data = new List<System.ArraySegment<'T>>() :> IList<System.ArraySegment<'T>>
        data.Add(segment)
        data

    type Socket with
        member this.MyAcceptAsync(receiveSize) =
            Async.FromBeginEnd(receiveSize,
                               (fun (receiveSize, callback, state) ->
                                   this.BeginAccept(receiveSize, callback, state)),
                               this.EndConnect)
        member this.MyConnectAsync(ipAddress : IPAddress, port : int) =
            Async.FromBeginEnd(ipAddress, port,
                               (fun (ipAddress:IPAddress, port, callback, state) ->
                                   this.BeginConnect(ipAddress, port, callback, state)),
                               this.EndConnect)
        member this.MySendAsync(data, flags : SocketFlags) =
            Async.FromBeginEnd(toIList data, flags, 
                               (fun (data : IList<System.ArraySegment<byte>>,
                                     flags : SocketFlags, callback, state) ->
                                         this.BeginSend(data, flags, callback, state)),
                               this.EndSend)
        member this.MyReceiveAsync(data, flags : SocketFlags) =
            Async.FromBeginEnd(toIList data, flags, 
                               (fun (data : IList<System.ArraySegment<byte>>,
                                     flags : SocketFlags, callback, state) ->
                                         this.BeginReceive(data, flags, callback, state)),
                               this.EndReceive)

    let port = 11000

    let socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    let ipHostEntry = Dns.Resolve("hostname.contoso.com")
    printfn "Server address: %s" (ipHostEntry.AddressList.[0].ToString())
    
    let connectSendReceive (socket : Socket) =
        async {
            do! socket.MyConnectAsync(ipHostEntry.AddressList.[0], 11000)
            printfn "Connected to remote host."
            let buffer1 = [| 0uy .. 255uy |]
            let buffer2 = Array.zeroCreate<byte> 255
            let flags = new SocketFlags()
            printfn "Sending data..."
            let! flag = socket.MySendAsync(buffer1, flags)
            printfn "Receiving data..."
            let! result = socket.MyReceiveAsync(buffer2, flags)
            printfn "Received data from remote host."
            return buffer2
        }

    let acceptReceiveSend (socket : Socket) =
        async {
            socket.Listen(1)
            do! socket.MyAcceptAsync(256)
            let buffer1 = Array.zeroCreate<byte> 255
            let flags = new SocketFlags()
            let! flag = socket.MyReceiveAsync(buffer1, flags)
            let buffer2 = Array.rev buffer1
            let! flag = socket.MySendAsync(buffer2, flags)
            return buffer2
        }

    let taskClient = Async.StartAsTask(connectSendReceive(socket))
    
    taskClient.Wait()
    taskClient.Result |> Array.iter (fun elem -> printf "%d " elem)