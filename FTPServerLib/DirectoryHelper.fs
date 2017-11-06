namespace FTPServerLib
open System.IO
open SessionInfo

module DirectoryHelpers =
    let directoryDetails path = 
        let dir = DirectoryInfo(path)
        
        let files = 
            dir.GetFiles()
            |> Array.map (fun f -> f.ToString() |> sprintf "File : %s")
        
        let folders = 
            dir.GetDirectories()
            |> Array.map (fun d -> d.ToString() |> sprintf "Dir : %s")
        Array.append files folders

    let getResponseToDir(sessionData : SessionData) =
        let filesAndFolders =
            sessionData.CurrentPath 
            |> directoryDetails
            |> String.concat "\n"
        "Dir : . \nDir : .. \n"+filesAndFolders
    
    let getFile file =
        let file = FileInfo(file)
        file

    let getFileContent file (sessionData : SessionData) =
        Path.Combine(sessionData.CurrentPath , file) 
        |> File.ReadAllLines
        |> String.concat "\n"