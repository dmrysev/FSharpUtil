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
    let guid = System.Guid.NewGuid().ToString()
    let temporaryScriptFile = "/tmp/" + guid + ".sh"
    System.IO.File.WriteAllText(temporaryScriptFile, command)
    let p = new System.Diagnostics.Process()
    p.StartInfo.RedirectStandardOutput <- true
    p.StartInfo.UseShellExecute <- false
    p.StartInfo.FileName <- "/bin/bash"
    p.StartInfo.Arguments <- temporaryScriptFile
    p.Start() |> ignore    
    let reader = p.StandardOutput
    let output = reader.ReadToEnd()
    p.WaitForExit()
    System.IO.File.Delete temporaryScriptFile
    output
