namespace FTPServerLib
open System.IO

module DirectoryHelpers =
    let pathToTest = "/Users/vr/WorkSpace/gist/" 
    let directoryDetails path = 
        let dir = DirectoryInfo(path)
        
        let files = 
            dir.GetFiles()
            |> Array.map (fun x -> x.ToString())
        
        let folders = 
            dir.GetDirectories()
            |> Array.map (fun x -> x.ToString())
        Array.append files folders

    let getFile file =
        let file = FileInfo(file)
        file