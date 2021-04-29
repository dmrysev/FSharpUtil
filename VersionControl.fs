module Util.VersionControl

let run (workingDirectory: string) (command: string) =
    let p = new System.Diagnostics.Process()
    p.StartInfo.RedirectStandardOutput <- false
    p.StartInfo.UseShellExecute <- false
    p.StartInfo.WorkingDirectory <- workingDirectory
    p.StartInfo.FileName <- "/bin/bash"
    let arguments = sprintf "-c \"%s\"" command
    p.StartInfo.Arguments <- arguments
    p.Start() |> ignore    
    p

let add(filePath: string) = 
    let workingDirectory = System.IO.Path.GetDirectoryName filePath
    let command = sprintf "git add %s" filePath
    let proc = run workingDirectory command
    proc.WaitForExit()

let commit (workingDirectory: string) (message: string) = 
    let command = sprintf "git commit -a -m '%s'" message
    let proc = run workingDirectory command
    proc.WaitForExit()