namespace FTPServerLib
open System.Net.Sockets

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
        | UNSUPPORTED

module SessionInfo =
    open FTPBasic
    
    type SessionData =
        {
            cmdHistory : string list
            currentPath : string
            stream : NetworkStream // remove it from the session data since its is not mutable.
            userName : string
        }

    let updateCmdHistory (sessionData : SessionData) (cmd : SupportedCommands) = 
        let newHistory = (string cmd)::sessionData.cmdHistory
        {sessionData with cmdHistory=newHistory}
    
    let updateCurrentPath (sessionData:SessionData) newPath = 
        {sessionData with currentPath=newPath}

    let updateUserName (sessionData:SessionData) name = 
         {sessionData with userName=name}