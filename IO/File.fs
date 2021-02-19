module Util.IO.File

open System.IO

let appendLine (filePath: string) (text: string) =
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
