namespace FTPServerLib
open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Threading

module ClientHelpers =
    let CreateClient(server:string, port:int) =  
        let localEndPoint = IPEndPoint(IPAddress.Parse(server), port) 
        let client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        client.Connect(localEndPoint)
        Console.WriteLine "Socket connected"
        printfn "Sending request ..."
        let mutable keepRunning = true
        while keepRunning do
            // Encode the data string into a byte array.  
            let userInput = Console.ReadLine()
            if userInput = "" then
                keepRunning <- false
            let msg = System.Text.Encoding.ASCII.GetBytes(userInput);  

            // Send the data through the socket.  
            let bytesSent = client.Send(msg);  
            printfn "The data of lentgth %d is sent." bytesSent

            // Receive the response from the remote device.  
            let buffer: byte [] = Array.zeroCreate 1024
            let bytesRec = client.Receive(buffer);  
            let respString = System.Text.Encoding.UTF8.GetString(buffer) 
 
            Console.WriteLine("Result : {0}",  respString)

        // Release the socket.  
        client.Shutdown(SocketShutdown.Both);  
        client.Close(); 
        printfn "Finally finished!"