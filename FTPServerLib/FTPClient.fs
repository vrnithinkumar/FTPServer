namespace FTPServerLib
open System
open System.Net.Sockets
open ServerHelpers
open System.Threading

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
        printfn "Socket Created!"
        let acceptedSocket = dataSendingSocket.Accept()
        printfn "Accepted data connection !"
        new NetworkStream(acceptedSocket, false)

    let handleRetr (userInput:string) =
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

    let handleInputAsync (stream, userInput:string) =
        async {
                match userInput.ToLower() with
                | "retr" ->  
                    writeCommandGetResult stream userInput
                    handleRetr userInput
                | "stor" ->
                    writeCommandGetResult stream userInput
                    handleStor userInput
                | "passive" ->
                    writeCommandGetResult stream userInput
                    sprintf "port %d" ServerConfiguration.dataPort |> writeToStream stream true
                | _ -> writeCommandGetResult stream userInput
        }

    let CreateClient() =  
        let client = createCommandSocket false
        let stream = new NetworkStream(client, false) 
        //writeToStream stream ""
        let mutable keepRunning = true
        let cancellationSource = new CancellationTokenSource()
        while keepRunning do
            // Encode the data string into a byte array.  
            let userInput = Console.ReadLine()
            let handleInput = handleInputAsync (stream, userInput)  

            match userInput.ToLower() with
                | "" | "close" -> 
                    Async.RunSynchronously(handleInput)
                    cancellationSource.Cancel()
                | "quit" -> 
                    Async.RunSynchronously(handleInputAsync(stream, "close"))
                    cancellationSource.Cancel()
                    keepRunning <- false
                | _ ->
                    Async.StartWithContinuations (
                        handleInput, 
                        (fun () -> printfn "Finished command execution !"),
                        (fun (x : exn) -> printfn "%s \n%s" x.Message x.StackTrace),
                        (fun (e : OperationCanceledException) -> printfn "Cancelled"),
                        cancellationSource.Token)

        // Releasing the socket.  
        stream.Close()
        client.Shutdown(SocketShutdown.Both);  
        client.Close(); 
        printfn "Finally finished!"