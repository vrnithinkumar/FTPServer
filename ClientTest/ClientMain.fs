﻿// Learn more about F# at http://fsharp.org

open System
open FTPServerLib.ClientHelpers
let port = 2121 
let localHost = "127.0.0.1"
[<EntryPoint>]
let main argv =
    printfn "Starting server for testing."
    CreateClient(localHost, port)
    0 // return an integer exit code
