namespace FTPServerLib
open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading
open ServerHelpers

module ClientHelpers =
    let CreateClient(server:string, port:int) =  
        let localEndPoint = IPEndPoint(IPAddress.Parse(server), port) 
        let client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        client.Connect(localEndPoint)
        Console.WriteLine "Socket connected"
        printfn "Sending request ..."
        let stream = new NetworkStream(client, false) 
        //writeToStream stream ""
        let mutable keepRunning = true
        while keepRunning do
            // Encode the data string into a byte array.  
            let userInput = Console.ReadLine()
            if userInput = "" then
                keepRunning <- false

            // Send the data through the socket.  
            userInput+"\r"
            |> writeToStream stream true
            System.Threading.Thread.Sleep 3000
            //printfn "Meggage is passed!"
            let respString = readFromStream stream
            printfn "Reply from server : %s " respString

        // Release the socket.  
        stream.Close()
        client.Shutdown(SocketShutdown.Both);  
        client.Close(); 
        printfn "Finally finished!"