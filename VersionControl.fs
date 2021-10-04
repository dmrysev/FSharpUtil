module Util.VersionControl

open Util.IO.Path

let run (workingDirectory: DirectoryPath) (command: string) =
    let p = new System.Diagnostics.Process()
    p.StartInfo.RedirectStandardOutput <- false
    p.StartInfo.UseShellExecute <- false
    p.StartInfo.WorkingDirectory <- workingDirectory.Value
    p.StartInfo.FileName <- "/bin/bash"
    let arguments = sprintf "-c \"%s\"" command
    p.StartInfo.Arguments <- arguments
    p.Start() |> ignore    
    p

let add (filePath: FilePath) = 
    let workingDirectory = filePath.DirectoryPath
    let command = sprintf "git add %s" filePath.Value
    let proc = run workingDirectory command
    proc.WaitForExit()

let commit (workingDirectory: DirectoryPath) (message: string) = 
    let command = sprintf "git commit -a -m '%s'" message
    let proc = run workingDirectory command
    proc.WaitForExit()