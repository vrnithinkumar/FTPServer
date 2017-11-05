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

    let createDataStream () =
        let dataSendingSocket = createDataSocket true
        let acceptedSocket = dataSendingSocket.Accept()
        new NetworkStream(acceptedSocket, false)


    let handleDataSocket (userInput:string) =
        let dataStream = createDataStream ()
                
        let fileData = readFromStream dataStream
        printfn "RETR result : %s " fileData 
        
        let pathToFile = userInput.Split ' ' |> Array.item 1
        writeToFile pathToFile fileData
        
        // cleaning up
        // dataStream.Close()
        // acceptedSocket.Shutdown(SocketShutdown.Both);  
        // acceptedSocket.Close(); 
        printfn "Finished data connection !"
    
    let handleStor (userInput:string) =
        let pathToFile = userInput.Split ' ' |> Array.item 1
        let data = 
            System.IO.Path.Combine(IO.Directory.GetCurrentDirectory(), pathToFile) 
            |> IO.File.ReadAllLines
            |> String.concat "\n"
        
        let dataStream = createDataStream ()
                
        writeToStream dataStream true data
        
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
                
            match userInput.ToLower() with
            | "retr" ->  
                writeCommandGetResult stream userInput
                handleDataSocket userInput
            | "stor" ->
                writeCommandGetResult stream userInput
                handleStor userInput
            | "passive" ->
                writeCommandGetResult stream userInput
                sprintf "port %d" ServerConfiguration.dataPort |> writeToStream stream true
            | _ -> writeCommandGetResult stream userInput

        // Releasing the socket.  
        stream.Close()
        client.Shutdown(SocketShutdown.Both);  
        client.Close(); 
        printfn "Finally finished!"