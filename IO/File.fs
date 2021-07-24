module Util.IO.File

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
    let dirPath = System.IO.Path.GetDirectoryName destinationPath
    System.IO.Directory.CreateDirectory dirPath |> ignore
    System.IO.File.Move(sourceFilePath, destinationPath)
