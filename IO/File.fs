module Util.IO.File

open Util.IO.Path
open System.IO

let create (filePath: string) =
    let dirPath = System.IO.Path.GetDirectoryName filePath
    System.IO.Directory.CreateDirectory dirPath |> ignore
    use fileStream = File.Create filePath
    ()

let appendLine (filePath: string) (text: string) =
    if not <| Util.IO.Path.exists filePath then create filePath
    use fileStream = File.Open(filePath, FileMode.Append)
    use streamWriter = new StreamWriter(fileStream)
    streamWriter.WriteLine(text)
    
let tail (filePath: string) (linesCount: int) =
    let command = sprintf "tail -n %i %s" linesCount filePath
    let p = Util.Process.run command
    p.WaitForExit()
    let output = p.StandardOutput.ReadToEnd().Replace("\r", "")
    output.Split('\n')

let lastLine (filePath: string) = tail filePath 1 |> Seq.head

let move (sourceFilePath: string) (destinationPath: string) =
    if destinationPath |> Util.IO.Path.exists && destinationPath |> Util.IO.Path.isDirectory then
        let fileName = System.IO.Path.GetFileName sourceFilePath
        let destinationFilePath = destinationPath/fileName
        System.IO.File.Move(sourceFilePath, destinationFilePath)
    else
        let dirPath = System.IO.Path.GetDirectoryName destinationPath
        System.IO.Directory.CreateDirectory dirPath |> ignore
        System.IO.File.Move(sourceFilePath, destinationPath)

let delete path =
    System.IO.File.Delete path
