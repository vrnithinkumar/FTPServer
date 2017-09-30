namespace FTPServerLib
module FTPCommands =
    type SupportedCommands =
        | LS
        | LOGIN
        | CLOSE
        | HELP
        | DIR
        | UNSUPPORTED
    let parseFTPCommand s = 
        match s with
        | "ls" -> SupportedCommands.LS
        | "login" -> SupportedCommands.LOGIN
        | "close" -> SupportedCommands.CLOSE
        | "help" -> SupportedCommands.HELP
        | "dir" -> SupportedCommands.DIR
        | _ -> SupportedCommands.UNSUPPORTED