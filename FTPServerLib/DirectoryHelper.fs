namespace FTPServerLib
open System.IO

module DirectoryHelpers =
    let pathToTest = Directory.GetCurrentDirectory()
    let directoryDetails path = 
        let dir = DirectoryInfo(path)
        
        let files = 
            dir.GetFiles()
            |> Array.map (fun x -> x.ToString())
        
        let folders = 
            dir.GetDirectories()
            |> Array.map (fun x -> x.ToString())
        Array.append files folders
    
    let getResponseToDir =
        let filesAndFolders =
            directoryDetails pathToTest
            |> String.concat "\n"
        ". \n..\n"+filesAndFolders
    
    let getFile file =
        let file = FileInfo(file)
        file

    let getTheCurrentDirectory =
        pathToTest