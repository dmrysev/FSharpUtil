module Util.IO.File

open Util.IO.Path
open System.IO

let create (filePath: FilePath) =
    let dirPath = filePath |> FilePath.directoryPath
    System.IO.Directory.CreateDirectory dirPath.Value |> ignore
    use fileStream = File.Create filePath.Value
    ()

let appendLine (filePath: FilePath) (text: string) =
    if not <| Util.IO.Path.FilePath.exists filePath then create filePath
    use fileStream = File.Open(filePath.Value, FileMode.Append)
    use streamWriter = new StreamWriter(fileStream)
    streamWriter.WriteLine(text)

let writeText (filePath: FilePath) (text: string) = 
    if not <| Util.IO.Path.FilePath.exists filePath then create filePath
    System.IO.File.WriteAllText (filePath.Value, text)
    
let writeLines (filePath: FilePath) (lines: seq<string>) = System.IO.File.WriteAllLines(filePath.Value, lines)

let head (filePath: FilePath) (linesCount: int) =
    let command = sprintf "head -n %i %s" linesCount filePath.Value
    let output = Util.Process.execute command
    output.Replace("\r", "").Split('\n')    

let tail (filePath: string) (linesCount: int) =
    let command = sprintf "tail -n %i %s" linesCount filePath
    let output = Util.Process.execute command
    output.Replace("\r", "").Split('\n')

let firstLine(filePath: FilePath) = head filePath 1 |> Seq.head 
let lastLine (filePath: string) = tail filePath 1 |> Seq.head

let move (sourceFilePath: FilePath) (destinationPath: string) =
    if destinationPath |> exists && destinationPath |> isDirectory then
        let destinationPath = DirectoryPath destinationPath
        let fileName = sourceFilePath |> FilePath.fileName
        let destinationFilePath = destinationPath/fileName
        System.IO.File.Move(sourceFilePath.Value, destinationFilePath.Value)
    else
        let destinationPath = FilePath destinationPath
        let dirPath = destinationPath |> FilePath.directoryPath
        System.IO.Directory.CreateDirectory dirPath.Value |> ignore
        System.IO.File.Move (sourceFilePath.Value, destinationPath.Value)

let moveToDirectory (sourceFilePath: FilePath) (destinationDirPath: DirectoryPath) =
    let fileName = sourceFilePath |> FilePath.fileName
    let destinationFilePath = destinationDirPath/fileName
    System.IO.File.Move(sourceFilePath.Value, destinationFilePath.Value)

let delete (path: FilePath) = System.IO.File.Delete path.Value

let copy (sourceFilePath: FilePath) destinationPath = 
    if destinationPath |> exists && destinationPath |> isDirectory then
        let destinationPath = DirectoryPath destinationPath
        let fileName = sourceFilePath |> FilePath.fileName
        let destinationFilePath = destinationPath/fileName
        System.IO.File.Copy(sourceFilePath.Value, destinationFilePath.Value, true)
    else
        System.IO.File.Copy(sourceFilePath.Value, destinationPath, true)

let exists (filePath: FilePath) = System.IO.File.Exists filePath.Value
let ensureExists (filePath: FilePath) = if not (filePath |> exists) then create filePath
let readAllLines (filePath: FilePath) = System.IO.File.ReadAllLines filePath.Value
let readAllText (filePath: FilePath) = System.IO.File.ReadAllText filePath.Value

let isSymbolicLink (filePath: FilePath) =
    Util.IO.Path.isSymbolicLink filePath.Value

let getSymbolicLinkRealPath (filePath: FilePath) =
    if not (isSymbolicLink filePath) then raise (System.ArgumentException "Not a symbolic link")
    let command = sprintf "readlink -f \"%s\"" filePath.Value
    let output = Util.Process.execute command
    output.Replace("\n", "")

let createSymbolicLink (sourcePath: FilePath) (destinationPath: string) =
    let command = sprintf "ln -s \"%s\" \"%s\"" sourcePath.Value destinationPath
    Util.Process.execute command |> ignore

let openWithDefaultApplication (filePath: FilePath) =
    if Util.Environment.OS.isLinux() then
        Util.Process.executeNoOutput $"xdg-open {filePath.Value} &"
    else raise (System.NotImplementedException())

let size (filePath: FilePath) =
    let fileInfo = System.IO.FileInfo (filePath.Value)
    fileInfo.Length

let readBytes (filePath: FilePath) = System.IO.File.ReadAllBytes(filePath.Value)
