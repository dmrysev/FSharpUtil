module Util.IO.Directory

open Util.Path

let empty (dirPath: DirectoryPath) =
    System.IO.Directory.GetFileSystemEntries(dirPath.Value).Length = 0

let exists (dirPath: DirectoryPath) =
    System.IO.Directory.Exists dirPath.Value

let isSymbolicLink (dirPath: DirectoryPath) =
    let pathInfo = System.IO.FileInfo dirPath.Value
    pathInfo.Attributes.HasFlag(System.IO.FileAttributes.ReparsePoint)

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

let listEntries (dirPath: DirectoryPath) =
    [ listDirectories dirPath |> Seq.map Path.Directory
      listFiles dirPath |> Seq.map Path.File ]
    |> Seq.concat

let copy (source: DirectoryPath) (destination: DirectoryPath) = raise (System.NotImplementedException "")

let move (source: DirectoryPath) (destination: DirectoryPath) = 
    let destination = 
        if destination |> exists then destination/(source |> DirectoryPath.directoryName)
        else destination
    System.IO.Directory.Move(source.Value, destination.Value)

let createSymbolicLink (sourcePath: DirectoryPath) (destinationPath: string) =
    let command = sprintf "ln -s \"%s\" \"%s\"" sourcePath.Value destinationPath
    Util.Process.execute command |> ignore

let getSymbolicLinkRealPath (path: DirectoryPath) =
    let command = sprintf "readlink -f \"%s\"" path.Value
    let output = Util.Process.execute command
    output.Replace("\n", "")

let modificationTime (path: DirectoryPath) =
    let dirInfo = System.IO.DirectoryInfo (path.Value)
    dirInfo.LastWriteTime

let creationTime (path: DirectoryPath) =
    let dirInfo = System.IO.DirectoryInfo (path.Value)
    dirInfo.CreationTime

let size (path: DirectoryPath) =
    System.IO.Directory.EnumerateFiles(path.Value, "*", System.IO.SearchOption.AllDirectories)
    |> Seq.map (fun filePath ->
        let fileInfo = System.IO.FileInfo (filePath)
        fileInfo.Length)
    |> Seq.sum
        
let realPath (dirPath: DirectoryPath) = 
    Util.Process.execute $"realpath '{dirPath.Value}'"
    |> DirectoryPath
