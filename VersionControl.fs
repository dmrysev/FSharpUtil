module Util.VersionControl

open Util.IO.Path

let run (workingDirectory: DirectoryPath) (command: string) =
    use p = new System.Diagnostics.Process()
    p.StartInfo.RedirectStandardOutput <- false
    p.StartInfo.UseShellExecute <- false
    p.StartInfo.WorkingDirectory <- workingDirectory.Value
    let p = p |> Util.Process.useBash command
    p.WaitForExit()

let evaluate (workingDirectory: DirectoryPath) (command: string) =
    use p = new System.Diagnostics.Process()
    p.StartInfo.WorkingDirectory <- workingDirectory.Value
    p 
    |> Util.Process.useBash command
    |> Util.Process.getOutput

let add (filePath: FilePath) = 
    let workingDirectory = filePath |> FilePath.directoryPath
    let command = sprintf "git add %s" filePath.Value
    run workingDirectory command

let addAll (workingDirectory: DirectoryPath) =
    let command = sprintf "git add ."
    run workingDirectory command

let commit (workingDirectory: DirectoryPath) (message: string) = 
    let command = sprintf "git commit -a -m '%s'" message
    run workingDirectory command

let currentRevision (workingDirectory: DirectoryPath) = evaluate workingDirectory "git rev-parse HEAD"
