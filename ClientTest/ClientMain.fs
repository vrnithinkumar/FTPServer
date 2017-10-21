open FTPServerLib.ClientHelpers

[<EntryPoint>]
let main argv =
    printfn "Starting client."
    CreateClient()
    0 