module Util.IO.Directory

open Util.IO.Path

let empty (dirPath: DirectoryPath) =
    System.IO.Directory.GetFileSystemEntries(dirPath.Value).Length = 0

let exists (dirPath: DirectoryPath) =
    System.IO.Directory.Exists dirPath.Value

let countFiles (dirPath: DirectoryPath) =
    System.IO.Directory.EnumerateFiles dirPath.Value |> Seq.length

let create (dirPath: DirectoryPath) = 
    System.IO.Directory.CreateDirectory dirPath.Value |> ignore

let delete (dirPath: DirectoryPath) =  
    if dirPath |> exists then 
        System.IO.Directory.Delete(dirPath.Value, true)

let generateTemporaryDirectory() =
    let tempDir = Util.Environment.SpecialFolder.temporary
    let dirName = DirectoryName (Util.Guid.generate())
    let dirPath = tempDir/dirName
    create dirPath
    dirPath

let listFiles (dirPath: DirectoryPath) = 
    System.IO.Directory.EnumerateFiles dirPath.Value
    |> Seq.map FilePath

let listDirectories (dirPath: DirectoryPath) =
    System.IO.Directory.EnumerateDirectories dirPath.Value
    |> Seq.map DirectoryPath

let copy (source: DirectoryPath) (destination: DirectoryPath) = raise (System.NotImplementedException "")

let move (source: DirectoryPath) (destination: DirectoryPath) = 
    System.IO.Directory.Move(source.Value, destination.Value)
