module Util.Process

let run (command: string) =
    let p = new System.Diagnostics.Process()
    p.StartInfo.RedirectStandardOutput <- true
    p.StartInfo.UseShellExecute <- false
    p.StartInfo.FileName <- "/bin/bash"
    let arguments = sprintf "-c \"%s\"" command
    p.StartInfo.Arguments <- arguments
    p.Start() |> ignore    
    p

let execute (command: string) =
    let p = new System.Diagnostics.Process()
    p.StartInfo.RedirectStandardOutput <- true
    p.StartInfo.UseShellExecute <- false
    p.StartInfo.FileName <- "/bin/bash"
    let arguments = sprintf "-c \"%s\"" command
    p.StartInfo.Arguments <- arguments
    p.Start() |> ignore    
    let reader = p.StandardOutput
    let output = reader.ReadToEnd()
    p.WaitForExit()
    output
