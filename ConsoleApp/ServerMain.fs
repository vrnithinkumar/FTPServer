open System
open FTPServerLib.Main
[<EntryPoint>]
let main argv =
    printfn "Server started ! \n Wating for Connections! \n"
    StartServer()
    0