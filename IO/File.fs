module Util.IO.File

open Util.IO.Path
open System.IO

let create (filePath: string) =
    let dirPath = System.IO.Path.GetDirectoryName filePath
    System.IO.Directory.CreateDirectory dirPath |> ignore
    use fileStream = File.Create filePath
    ()

let appendLine (filePath: string) (text: string) =
    if not <| exists filePath then create filePath
    use fileStream = File.Open(filePath, FileMode.Append)
    use streamWriter = new StreamWriter(fileStream)
    streamWriter.WriteLine(text)

let head (filePath: string) (linesCount: int) =
    let command = sprintf "head -n %i %s" linesCount filePath
    let output = Util.Process.execute command
    output.Replace("\r", "").Split('\n')    

let tail (filePath: string) (linesCount: int) =
    let command = sprintf "tail -n %i %s" linesCount filePath
    let output = Util.Process.execute command
    output.Replace("\r", "").Split('\n')

let firstLine(filePath: string) = head filePath 1 |> Seq.head 

let lastLine (filePath: string) = tail filePath 1 |> Seq.head

let move (sourceFilePath: string) (destinationPath: string) =
    if destinationPath |> exists && destinationPath |> isDirectory then
        let fileName = System.IO.Path.GetFileName sourceFilePath
        let destinationFilePath = destinationPath/fileName
        System.IO.File.Move(sourceFilePath, destinationFilePath)
    else
        let dirPath = System.IO.Path.GetDirectoryName destinationPath
        System.IO.Directory.CreateDirectory dirPath |> ignore
        System.IO.File.Move(sourceFilePath, destinationPath)

let delete path =
    System.IO.File.Delete path

let copy sourceFilePath destinationPath = 
    if destinationPath |> exists && destinationPath |> isDirectory then
        let fileName = System.IO.Path.GetFileName sourceFilePath
        let destinationFilePath = destinationPath/fileName
        System.IO.File.Copy(sourceFilePath, destinationFilePath)
    else
        System.IO.File.Copy(sourceFilePath, destinationPath)

let popFirstLine (filePath: string) = 
    let firstLine = firstLine filePath
    let tempFile = Path.GetTempFileName()
    let linesToKeep = 
        File.ReadLines filePath
        |> Seq.skip 1
    File.WriteAllLines(tempFile, linesToKeep)
    File.Delete(filePath)
    File.Move(tempFile, filePath)
    firstLine
