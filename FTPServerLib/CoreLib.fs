namespace FTPServerLib

module FTPBasic =
    type SupportedCommands =
        | PWD
        | USER of string
        | PASS of string
        | CLOSE
        | HELP
        | LIST 
        | CD of string
        | RETR of string
        | PORT of int
        | STOR of string
        | PASSIVE
        | ACTIVE
        | UNSUPPORTED

module SessionInfo =
    open FTPBasic
    
    // A record type
    type SessionData =
        {
            cmdHistory : string list
            currentPath : string
            userName : string
            port : int option // make this as option type.
            passiveModeOn : bool
        }

    let updateCmdHistory (sessionData : SessionData) (cmd : SupportedCommands) = 
        let newHistory = (string cmd)::sessionData.cmdHistory
        {sessionData with cmdHistory = newHistory}
    
    let updateCurrentPath (sessionData:SessionData) newPath = 
        {sessionData with currentPath = newPath}

    let updateUserName (sessionData:SessionData) name = 
         {sessionData with userName = name}

    let updatePort (sessionData:SessionData) port = 
         {sessionData with port = port}

    let updateMode (sessionData:SessionData) mode = 
         {sessionData with passiveModeOn = mode}