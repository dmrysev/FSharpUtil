module Util.IO.Directory

open Util.IO.Path

let empty (dirPath: DirectoryPath) =
    System.IO.Directory.GetFileSystemEntries(dirPath.Value).Length = 0

let exists (dirPath: DirectoryPath) =
    System.IO.Directory.Exists dirPath.Value

let isSymbolicLink (dirPath: DirectoryPath) =
    Util.IO.Path.isSymbolicLink dirPath.Value

let countFiles (dirPath: DirectoryPath) =
    System.IO.Directory.EnumerateFiles dirPath.Value |> Seq.length

let create (dirPath: DirectoryPath) = 
    System.IO.Directory.CreateDirectory dirPath.Value |> ignore

let ensureExists (dirPath: DirectoryPath) = if not (dirPath |> exists) then create dirPath

let delete (dirPath: DirectoryPath) = 
    if dirPath |> exists then 
        System.IO.Directory.Delete(dirPath.Value, true)

let deleteSymbolicLink (dirPath: DirectoryPath) = 
    if dirPath |> isSymbolicLink then
        dirPath.Value |> FilePath |> Util.IO.File.delete

let generateTemporaryDirectory() =
    let tempDir = Util.Environment.SpecialFolder.temporary
    let dirName = DirectoryName (Util.Guid.generate())
    let dirPath = tempDir/dirName
    create dirPath
    dirPath

let listFiles (dirPath: DirectoryPath) = 
    System.IO.Directory.EnumerateFiles dirPath.Value
    |> Seq.map FilePath

let listFilesRecursive (dirPath: DirectoryPath) = 
    System.IO.Directory.EnumerateFiles(dirPath.Value, "*", System.IO.SearchOption.AllDirectories)
    |> Seq.map FilePath

let listFilesRecursivePattern (dirPath: DirectoryPath) (pattern: string) = 
    System.IO.Directory.EnumerateFiles(dirPath.Value, pattern, System.IO.SearchOption.AllDirectories)
    |> Seq.map FilePath

let listDirectories (dirPath: DirectoryPath) =
    System.IO.Directory.EnumerateDirectories dirPath.Value
    |> Seq.map DirectoryPath

let listDirectoriesRecursive (dirPath: DirectoryPath) =
    System.IO.Directory.EnumerateDirectories (dirPath.Value, "*", System.IO.SearchOption.AllDirectories)
    |> Seq.map DirectoryPath

let copy (source: DirectoryPath) (destination: DirectoryPath) = raise (System.NotImplementedException "")

let move (source: DirectoryPath) (destination: DirectoryPath) = 
    let destination = 
        if destination |> exists then destination/source.DirectoryName
        else destination
    System.IO.Directory.Move(source.Value, destination.Value)

let createSymbolicLink (sourcePath: DirectoryPath) (destinationPath: string) =
    let command = sprintf "ln -s \"%s\" \"%s\"" sourcePath.Value destinationPath
    Util.Process.execute command |> ignore

let getSymbolicLinkRealPath (path: DirectoryPath) =
    let command = sprintf "readlink -f \"%s\"" path.Value
    let output = Util.Process.execute command
    output.Replace("\n", "")
