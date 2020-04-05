module Util.FileSystem

open System.IO

let (/) path1 path2 = Path.Combine(path1, path2)

let fixFileName (fileName: string) =
    let invalidChars = Path.GetInvalidFileNameChars()
    Util.String.strip invalidChars fileName

let isDirectoryEmpty (folderPath: string) =
    Directory.GetFileSystemEntries(folderPath).Length = 0

let findAvailablePathWithAppendix (filePath: string) = 
    let rec findAppendix number =
        let newFilePath = sprintf "%s_%i" filePath number
        if File.Exists(newFilePath) || Directory.Exists(newFilePath)
        then findAppendix (number + 1)
        else newFilePath
    findAppendix 1