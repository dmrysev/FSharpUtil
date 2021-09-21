module Util.IO.Path

let (/) path1 path2 = System.IO.Path.Combine(path1, path2)

type FilePath(str: string) =
    let value' = 
        if str = "" then raise (System.ArgumentException("Path can't be empty"))
        str

    member this.Value = value'

    static member value (dirPath: FilePath) = dirPath.Value

type DirectoryPath(str: string) =
    let value' = 
        if str = "" then raise (System.ArgumentException("Path can't be empty"))
        str

    member this.Value = value'

    static member value (dirPath: DirectoryPath) = dirPath.Value

    static member (/+) (path1: DirectoryPath, path2: DirectoryPath) = 
        let combined = System.IO.Path.Combine(path1.Value, path2.Value)
        DirectoryPath combined

    static member (/+) (path1: DirectoryPath, path2: FilePath) = 
        let combined = System.IO.Path.Combine(path1.Value, path2.Value)
        FilePath combined

let fixFileName (fileName: string) =
    let invalidChars = ['/'; '<'; '>'; ':'; '"'; '/'; '\\'; '|'; '?'; '*']
    Util.String.strip invalidChars fileName

let fixPath (url: string) =
    if url.EndsWith "/" then url.Remove(url.Length - 1, 1)
    else url

let exists (path: string) = 
    System.IO.Directory.Exists path || System.IO.File.Exists path

let findAvailablePathWithAppendix (filePath: string) = 
    let rec findAppendix number =
        let newFilePath = sprintf "%s_%i" filePath number
        if exists newFilePath
        then findAppendix (number + 1)
        else newFilePath
    findAppendix 1

let isDirectory path =
    let attributes = System.IO.File.GetAttributes path
    attributes.HasFlag(System.IO.FileAttributes.Directory)

let isSymbolicLink path =
    let pathInfo = System.IO.FileInfo path
    pathInfo.Attributes.HasFlag(System.IO.FileAttributes.ReparsePoint)    

let getSymbolicLinkRealPath path =
    let command = sprintf "readlink -f '%s'" path
    let output = Util.Process.execute command
    output.Replace("\n", "")

let createSymbolicLink (sourcePath: string) destinationPath =
    let command = sprintf "ln -s \"%s\" \"%s\"" sourcePath destinationPath
    Util.Process.execute command |> ignore

let replaceFileName (fileName: string) (newFileName: string) =
    let extension = System.IO.Path.GetExtension fileName
    newFileName + extension
