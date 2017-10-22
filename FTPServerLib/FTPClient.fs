namespace FTPServerLib
open System
open System.Net.Sockets
open ServerHelpers

module ClientHelpers =
    let retrCommandName = "retr"
    let writeCommandGetResult stream userInput =
        userInput+"\r"
        |> writeToStream stream true
        System.Threading.Thread.Sleep 3000
        let respString = readFromStream stream
        printfn "Reply from server : %s " respString

    let handleDataSocket (userInput:string) =
        let dataSendingSocket = createDataSocket true
        let acceptedSocket = dataSendingSocket.Accept()
        let dataStream = new NetworkStream(acceptedSocket, false) 
                
        let fileData = readFromStream dataStream
        printfn "CAT result : %s " fileData 
        
        let pathToFile = userInput.Split ' ' |> Array.item 1
        writeToFile pathToFile fileData
        
        // cleaning up
        dataStream.Close()
        acceptedSocket.Shutdown(SocketShutdown.Both);  
        acceptedSocket.Close(); 
        printfn "Finished data connection !"
        
    let CreateClient() =  
        let client = createCommandSocket false
        let stream = new NetworkStream(client, false) 
        //writeToStream stream ""
        let mutable keepRunning = true
        while keepRunning do
            // Encode the data string into a byte array.  
            let userInput = Console.ReadLine()
            
            match userInput.ToLower() with
            | "" | "close" -> keepRunning <- false
            | _ -> ()
                
            match userInput.ToLower().Contains(retrCommandName) with
            | true ->  
                writeCommandGetResult stream userInput
                handleDataSocket userInput 
            | _ -> writeCommandGetResult stream userInput

        // Releasing the socket.  
        stream.Close()
        client.Shutdown(SocketShutdown.Both);  
        client.Close(); 
        printfn "Finally finished!"