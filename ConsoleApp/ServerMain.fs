// Learn more about F# at http://fsharp.org

open System
open FTPServerLib.Main
[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    StartServer()
    //let dirDetails = directoryDetails pathToTest
    //printfn "Details of dir : %A" dirDetails
    0 // return an integer exit code
