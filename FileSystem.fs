module Util.FileSystem

open System.IO

let (/) path1 path2 = Path.Combine(path1, path2)

let fixFileName (fileName: string) =
    let invalidChars = ['/'; '<'; '>'; ':'; '"'; '/'; '\\'; '|'; '?'; '*']
    Util.String.strip invalidChars fileName

let isDirectoryEmpty (folderPath: string) =
    Directory.GetFileSystemEntries(folderPath).Length = 0

let isDirectoryNotEmpty (folderPath: string) = 
    not <| isDirectoryEmpty folderPath

let isPathExists (path: string) = 
    Directory.Exists path || File.Exists path

let isPathNotExists (path: string) = 
    not <| isPathExists path

let findAvailablePathWithAppendix (filePath: string) = 
    let rec findAppendix number =
        let newFilePath = sprintf "%s_%i" filePath number
        if File.Exists(newFilePath) || Directory.Exists(newFilePath)
        then findAppendix (number + 1)
        else newFilePath
    findAppendix 1

let homeDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)

let countFiles (folderPath: string) =
    Directory.EnumerateFiles folderPath |> Seq.length
