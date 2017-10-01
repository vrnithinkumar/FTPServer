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
  let port = 2121
  let localHost = "127.0.0.1"
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

  let StartServer() =
    let localEndPoint = IPEndPoint(IPAddress.Parse(localHost), port)  
    let socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    socket.Bind(localEndPoint)
    socket.Listen(111)  
    printfn "Waiting for request ..."
    
    let socket = socket.Accept()
    let stream = new NetworkStream(socket) 
    while true do
      readAndWriteToStream stream
    
    socket.Shutdown(SocketShutdown.Both)
    socket.Close()
    printfn "Finally finished!"