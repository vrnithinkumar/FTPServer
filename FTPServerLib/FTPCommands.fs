namespace FTPServerLib
open DirectoryHelpers

module FTPCommands =
    let singleCommands = ["pwd" ; "close"; "ls" ; "help"]
    type SupportedCommands =
        | PWD
        | USER of string
        | PASS of string
        | CLOSE
        | HELP
        | CD of string
        | UNSUPPORTED

    let parseFTPCommand command = 
        printfn "Parsing %s" command
        let isSingleCommand =
            singleCommands
            |> List.contains command
        if isSingleCommand then
            match command.ToLower() with
            | "pwd" -> PWD
            | "close" -> CLOSE
            | "help" -> HELP
            | _ -> UNSUPPORTED
        else
            let [| cmdName; cmdArgs |] = command.Split ' '
            match cmdName.ToLower() with
            | "user" -> let userName = cmdArgs in USER userName        // ---> USER slacker   ---> PASS XXXX   ---> PORT 192,168,150,80,14,178
            | "pass" -> let password = cmdArgs in PASS password
            | "dir" -> let directory = cmdArgs in CD directory
            | _ -> UNSUPPORTED
 
    let getResponseByParsing commandString =
        let command = parseFTPCommand commandString
        printfn "Command %s is %A" commandString command
        match command with
            | CLOSE -> "Connections is closed"
            //| DIR   -> getResponseToDir
            | SupportedCommands.PWD   -> getTheCurrentDirectory
            | SupportedCommands.HELP  -> "Supported Commands are \n ls \n login \n close \n help \n dir"
            | SupportedCommands.UNSUPPORTED  -> "Error! \n Not supported!"

    let getResponseToDir =
        let filesAndFolders =
            directoryDetails pathToTest
            |> String.concat "\n"
        ". \n..\n" + filesAndFolders
    
    let getResponse commandString =
        printfn "Creating response for %s" commandString
        let trimmedString =commandString.Trim()
        let response =
            match commandString with
                | "login" -> 
                    printfn "Matched : %s " commandString
                    "Login as anonymous user!"
                | "close" -> 
                    printfn "Matched : %s " commandString
                    "Connections is closed"
                | "dir" -> 
                    printfn "Matched : %s " commandString
                    getResponseToDir
                | "pwd" -> 
                    printfn "Matched : %s " commandString
                    getTheCurrentDirectory
                | "help" -> 
                    printfn "Matched help : %s" commandString 
                    "Supported Commands are \n ls \n login \n close \n help \n dir"
                | _ ->
                    printfn "Matched  __: %s " commandString 
                    "Error! \n Not supported!"
                    
        printfn "Resp is ##%s" response
        response