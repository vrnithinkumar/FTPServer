open FTPServerLib.ClientHelpers

[<EntryPoint>]
let main argv =
    printfn "Starting client!\n Connecting to server...\n"
    CreateClient()
    0 