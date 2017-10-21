namespace FTPServerLib
open System
open System.Net.Sockets
open ServerHelpers

module ClientHelpers =
    let CreateClient() =  
        let client = createCommandSocket false
        Console.WriteLine "Socket connected"
        printfn "Sending request ..."
        let stream = new NetworkStream(client, false) 
        //writeToStream stream ""
        let mutable keepRunning = true
        while keepRunning do
            // Encode the data string into a byte array.  
            let userInput = Console.ReadLine()
            
            match userInput.ToLower() with
            | "" | "close" -> keepRunning <- false
            | _ -> ()
                
            match userInput.Contains("cat") with
            | true ->  
                let dataSendingSocket = createDataSocket true

                userInput+"\r"
                |> writeToStream stream true
                printfn "Done write inside cat "

                let respString = readFromStream stream
                printfn "Reply from server : %s " respString
                
                let acceptedSocket = dataSendingSocket.Accept()
                let dataStream = new NetworkStream(acceptedSocket, false) 
                
                let fileData = readFromStream dataStream
                printfn "CAT result : %s " fileData 
            | _ -> 
                userInput+"\r"
                |> writeToStream stream true
                System.Threading.Thread.Sleep 3000
                let respString = readFromStream stream
                printfn "Reply from server : %s " respString

        // Releasing the socket.  
        stream.Close()
        client.Shutdown(SocketShutdown.Both);  
        client.Close(); 
        printfn "Finally finished!"