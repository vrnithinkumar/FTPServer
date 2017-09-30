namespace FTPServerLib
open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading

// All socket and conneection related stuff.
module ServerHelpers =
  let port = 2121
  let localHost = "127.0.0.1"
  let readAndWriteToStream (s:NetworkStream) =
    let buffer: byte [] = Array.zeroCreate 1024
    let readLen = s.Read(buffer,0, 1024)
    let respString = System.Text.Encoding.UTF8.GetString(buffer) 
    printfn "Received request data : %s " respString
    let timeNow = 
      System.DateTime.Now.ToLongTimeString()
      |> System.Text.Encoding.UTF8.GetBytes
    let response = [|
      "HTTP/1.1 200 OK\r\n"B
      "Content-Type: text/plain\r\n"B
      "\r\n"B
      timeNow
      "\r \n Hello World!"B |] |> Array.concat
    s.Write(response, 0, response.Length)
  let StartServer() =
    let localEndPoint = IPEndPoint(IPAddress.Parse(localHost), port)  
    let s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    s.Bind(localEndPoint)
    s.Listen(111)  
    printfn "Waiting for request ..."
    
    let socket = s.Accept()
    let stream = new NetworkStream(socket) 
    while true do
      readAndWriteToStream stream
    
    s.Shutdown(SocketShutdown.Both)
    s.Close()
    printfn "Finally finished!"