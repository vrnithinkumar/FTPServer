namespace FTPServerLib
open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading

module ServerHelpers =
    type Socket with
        member socket.AsyncAccept() = Async.FromBeginEnd(socket.BeginAccept, socket.EndAccept)
    let CreateSocket port =
        let localEndPoint = IPEndPoint(IPAddress.Parse("127.0.0.1"), port);  
        let s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        s.Bind(localEndPoint)
        s
    let StartServer() =
        let port = 11702
        let s = CreateSocket port
        s.Listen(111)  
        let rec loop() = async {
          printfn "Waiting for request ..."
          let! socket = s.AsyncAccept()
          // Setting ownsSocket to false allows us to later re-use a socket.
          let stream = new NetworkStream(socket, false) 
          printfn "Received request"
          let response = [|
            "HTTP/1.1 200 OK\r\n"B
            "Content-Type: text/plain\r\n"B
            "\r\n"B
            "Hello World!"B |] |> Array.concat
          try
            try
              let! bytesSent = stream.AsyncWrite(response)
              printfn "Sent response"
            with e -> printfn "An error occurred: %s" e.Message
          finally
            stream.Close()
            socket.Shutdown(SocketShutdown.Both)
            socket.Close()
          return! loop() 
          }

        Async.Start(loop())
        printfn "created socket"

    StartServer()