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
            command.Split ' '
            |> Array.length < 2
        
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
            | DIR   -> getResponseToDir
            | SupportedCommands.PWD   -> getTheCurrentDirectory
            | SupportedCommands.HELP  -> "Supported Commands are \n ls \n login \n close \n help \n dir"
            | SupportedCommands.UNSUPPORTED  -> "Error! \n Not supported!"

    let getResponseToDir =
        let filesAndFolders =
            directoryDetails pathToTest
            |> String.concat "\n"
        ". \n..\n" + filesAndFolders

    type ServerReturnCodeEnum =
        | FTPServeReady = 220
        | PasswordRequest = 331 
        | UserLoggedIn = 230
        | NameSystemTyp = 215
        | Successfull = 200
        | FileStatusOkay = 150
        | ClosingDataConnection = 226
        | ClosingControlConnection = 221
        | InvalidCredential = 430

    let GetServerReturnMessageWithCode code =
        match code with
        | ServerReturnCodeEnum.FTPServeReady            -> "220 FTP server ready."
        | ServerReturnCodeEnum.PasswordRequest          -> "331 Password required." 
        | ServerReturnCodeEnum.UserLoggedIn             -> "230 user logged in."
        | ServerReturnCodeEnum.NameSystemTyp            -> "215"
        | ServerReturnCodeEnum.Successfull              -> "200 PORT command successful."
        | ServerReturnCodeEnum.FileStatusOkay           -> "150"
        | ServerReturnCodeEnum.ClosingDataConnection    -> "226 Transfer complete."
        | ServerReturnCodeEnum.ClosingControlConnection -> "221 Goodbye."
        | ServerReturnCodeEnum.InvalidCredential        -> "430 Inavalid user name or password."