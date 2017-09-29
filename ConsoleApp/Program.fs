// Learn more about F# at http://fsharp.org

open System
open FTPServerLib.ServerHelpers
[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    StartServer()
    0 // return an integer exit code
