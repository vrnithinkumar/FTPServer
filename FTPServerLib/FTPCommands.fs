namespace FTPServerLib
open FTPBasic
open SessionInfo
open DirectoryHelpers

module FTPCommands =
    let parseFTPCommand command = 
        printfn "Parsing %s" command

        match command.ToLower().Split ' ' with 
        | [|cmd|] ->
            match cmd with
            | "pwd" -> PWD
            | "close" -> CLOSE
            | "help" -> HELP
            | "ls" -> LIST
            | "passive" -> PASSIVE
            | "active" -> ACTIVE
            | _ -> UNSUPPORTED
        | [| cmdName; cmdArgs |] ->
            match cmdName with
            | "user" -> let userName = cmdArgs in USER userName
            | "pass" -> let password = cmdArgs in PASS password
            | "cd" -> let directory = cmdArgs in CD directory
            | "retr" -> let fileName = cmdArgs in RETR fileName
            | "stor" -> let fileName = cmdArgs in STOR fileName
            | "port" -> let port = cmdArgs in PORT (int port)
            | _ -> UNSUPPORTED
        | _ -> failwithf "Error! Unsupported command format."


    let getResponseByParsing commandString (sessionData : SessionData) =
        let command = parseFTPCommand commandString
        printfn "Command %s is %A" commandString command
        match command with
            | CLOSE -> "Connection is closed"
            | PWD   -> getResponseToDir sessionData
            | HELP  -> "Supported Commands are \n ls \n login \n close \n help \n dir"
            | UNSUPPORTED  -> "Error! \n Not supported!"
            | _ -> failwith "Not supported yet!"
    
    let responseToDir (sessionData : SessionData)=
        let filesAndFolders =
            sessionData.currentPath
            |> directoryDetails 
            |> String.concat "\n"
        "Dir : . \n Dir : ..\n" + filesAndFolders

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

    let getServerReturnMessageWithCode code =
        match code with
        | ServerReturnCodeEnum.FTPServeReady            as s -> sprintf "%d FTP server ready." ((int)s)
        | ServerReturnCodeEnum.PasswordRequest          as s -> sprintf "%d  Password required."  ((int)s)
        | ServerReturnCodeEnum.UserLoggedIn             as s -> sprintf "%d  user logged in." ((int)s)
        | ServerReturnCodeEnum.NameSystemTyp            as s -> sprintf "%d" ((int)s)
        | ServerReturnCodeEnum.Successfull              as s -> sprintf "%d  PORT command successful."  ((int)s)  
        | ServerReturnCodeEnum.FileStatusOkay           as s -> sprintf "%d " ((int)s)
        | ServerReturnCodeEnum.ClosingDataConnection    as s -> sprintf "%d  Transfer complete." ((int)s)
        | ServerReturnCodeEnum.ClosingControlConnection as s -> sprintf "%d  Goodbye." ((int)s)
        | ServerReturnCodeEnum.InvalidCredential        as s -> sprintf "%d  Inavalid user name or password." ((int)s)
        | _ -> failwith "Server return messages is not yet supported!" 

