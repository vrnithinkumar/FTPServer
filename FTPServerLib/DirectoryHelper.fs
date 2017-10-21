namespace FTPServerLib
open System.IO

module DirectoryHelpers =
    let mutable _currentPath = ""
    
    let currentDirectory() = 
        match _currentPath with
        | "" -> Directory.GetCurrentDirectory()
        | _ -> _currentPath

    let changeCurrentDirectory newPath = 
        _currentPath <- newPath    
        printfn "Changing dir to %s" _currentPath

    let directoryDetails path = 
        let dir = DirectoryInfo(path)
        
        let files = 
            dir.GetFiles()
            |> Array.map (fun f -> f.ToString() |> sprintf "File : %s")
        
        let folders = 
            dir.GetDirectories()
            |> Array.map (fun d -> d.ToString() |> sprintf "Dir : %s")
        Array.append files folders
    
    let getResponseToDir() =
        let filesAndFolders =
            currentDirectory() 
            |> directoryDetails 
            |> String.concat "\n"
        ". \n..\n"+filesAndFolders
    
    let getFile file =
        let file = FileInfo(file)
        file
    let getFileContent file =
        Path.Combine(currentDirectory(), file) 
        |> File.ReadAllLines
        |> String.concat "\n"