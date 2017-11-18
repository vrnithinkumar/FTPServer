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
            CmdHistory : string list
            CurrentPath : string
            UserName : string
            Port : int option 
            PassiveModeOn : bool
        }

    let updateCmdHistory (sessionData : SessionData) (cmd : SupportedCommands) = 
        let newHistory = (string cmd)::sessionData.CmdHistory
        {sessionData with CmdHistory = newHistory}
    
    let updateCurrentPath (sessionData:SessionData) newPath = 
        {sessionData with CurrentPath = newPath}

    let updateUserName (sessionData:SessionData) name = 
         {sessionData with UserName = name}

    let updatePort (sessionData:SessionData) port = 
         {sessionData with Port = port}

    let updateMode (sessionData:SessionData) mode = 
         {sessionData with PassiveModeOn = mode}