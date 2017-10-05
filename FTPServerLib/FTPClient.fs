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
        use stream = new NetworkStream(client) 
        let mutable keepRunning = true
        while keepRunning do
            // Encode the data string into a byte array.  
            let userInput = Console.ReadLine()
            if userInput = "" then
                keepRunning <- false
            let msg = System.Text.Encoding.ASCII.GetBytes(userInput+"\r");  

            // Send the data through the socket.  
            stream.Write (msg, 0, msg.Length)
            printfn "Meggage is passed!"

            let respString = readFromStream stream
            printfn "Reply from server : %s " respString

        // Release the socket.  
        client.Shutdown(SocketShutdown.Both);  
        client.Close(); 
        printfn "Finally finished!"