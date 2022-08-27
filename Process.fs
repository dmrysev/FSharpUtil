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

let withBashScriptExecute (command: string) (p: System.Diagnostics.Process) =
    let guid = System.Guid.NewGuid().ToString()
    let temporaryScriptFile = "/tmp/" + guid + ".sh"
    System.IO.File.WriteAllText(temporaryScriptFile, command)
    p.Exited.Add (fun _ -> System.IO.File.Delete temporaryScriptFile)
    p.StartInfo.FileName <- "/bin/bash"
    p.StartInfo.Arguments <- temporaryScriptFile
    p

let execute (command: string) =
    use p = new System.Diagnostics.Process() |> withBashScriptExecute command
    p.StartInfo.RedirectStandardOutput <- true
    p.StartInfo.RedirectStandardError <- true
    p.StartInfo.UseShellExecute <- false
    p.Start() |> ignore    
    let reader = p.StandardOutput
    let output = reader.ReadToEnd()
    let errorReader = p.StandardError
    let errorOutput = errorReader.ReadToEnd()
    p.WaitForExit()
    if errorOutput <> "" then raise (System.SystemException errorOutput)
    if output.EndsWith("\n") then output.Remove(output.Length - 1)
    else output

let executeNoOutput (command: string) =
    let guid = System.Guid.NewGuid().ToString()
    let temporaryScriptFile = "/tmp/" + guid + ".sh"
    System.IO.File.WriteAllText(temporaryScriptFile, command)
    let p = new System.Diagnostics.Process()
    p.StartInfo.RedirectStandardInput <- false
    p.StartInfo.RedirectStandardOutput <- false
    p.StartInfo.RedirectStandardError <- false
    p.StartInfo.UseShellExecute <- false
    p.StartInfo.FileName <- "/bin/bash"
    p.StartInfo.Arguments <- temporaryScriptFile
    p.Start() |> ignore
    p.WaitForExit()
    System.IO.File.Delete temporaryScriptFile
