namespace FTPServerLib
open DirectoryHelpers

module FTPCommands =
    type SupportedCommands =
        | PWD
        | LOGIN
        | CLOSE
        | HELP
        | DIR
        | UNSUPPORTED

    let parseFTPCommand command = 
        printfn "Parsing %s" command
        match command with
        | "pwd" -> SupportedCommands.PWD
        | "login" -> SupportedCommands.LOGIN
        | "close" -> SupportedCommands.CLOSE
        | "help" -> SupportedCommands.HELP
        | "dir" -> SupportedCommands.DIR
        | _ -> SupportedCommands.UNSUPPORTED
 
    let getResponseByParsing commandString =
        let command = parseFTPCommand commandString
        printfn "Command %s is %A" commandString command
        match command with
            | SupportedCommands.LOGIN -> "Login as anonymous user!"
            | SupportedCommands.CLOSE -> "Connections is closed"
            | SupportedCommands.DIR   -> getResponseToDir
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
        let trimmedString = commandString.Trim()
        let isSame = trimmedString = commandString
        printfn "Is Same?  ; %b" isSame
        let response =
            match trimmedString with
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

    let Test ()= 
       let commandBuffer = System.Text.Encoding.UTF8.GetBytes("help")
       let ftpCommand = System.Text.Encoding.UTF8.GetString(commandBuffer)
       let command = getResponse ftpCommand
       printfn "Command %s is %A" "help" command