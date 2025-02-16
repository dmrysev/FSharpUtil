module Util.Process

let useBash (command: string) (p: System.Diagnostics.Process) =
    p.StartInfo.FileName <- "bash"
    let arguments = sprintf "-c \"%s\"" command
    p.StartInfo.Arguments <- arguments
    p

let useBashScript (command: string) (p: System.Diagnostics.Process) =
    let guid = System.Guid.NewGuid().ToString()
    let tempPath = System.IO.Path.GetTempPath()
    let temporaryScriptFile = tempPath + "/" + guid + ".sh"
    System.IO.File.WriteAllText(temporaryScriptFile, command)
    p.Exited.Add (fun _ -> System.IO.File.Delete temporaryScriptFile)
    p.StartInfo.FileName <- "bash"
    p.StartInfo.Arguments <- temporaryScriptFile
    p

let noRedirect (p: System.Diagnostics.Process) =
    p.StartInfo.RedirectStandardInput <- false
    p.StartInfo.RedirectStandardOutput <- false
    p.StartInfo.RedirectStandardError <- false
    p

let redirectOutput (p: System.Diagnostics.Process) =
    p.StartInfo.RedirectStandardOutput <- true
    p.StartInfo.RedirectStandardError <- true
    p.StartInfo.UseShellExecute <- false
    p

let getOutput (p: System.Diagnostics.Process) =
    let p = p |> redirectOutput
    p.Start() |> ignore    
    let reader = p.StandardOutput
    let output = reader.ReadToEnd()
    let errorReader = p.StandardError
    let errorOutput = errorReader.ReadToEnd()
    p.WaitForExit()
    if errorOutput <> "" then raise (System.SystemException errorOutput)
    if output.EndsWith("\n") then output.Remove(output.Length - 1)
    else output

let run (command: string) =
    use p = 
        new System.Diagnostics.Process()
        |> useBashScript command
        |> noRedirect
    p.Start() |> ignore
    p.WaitForExit()

let execute (command: string) = new System.Diagnostics.Process() |> useBashScript command |> getOutput

let executeNoOutput (command: string) =
    use p = 
        new System.Diagnostics.Process() 
        |> useBashScript command
        |> noRedirect
    p.StartInfo.UseShellExecute <- false
    p.Start() |> ignore
    p.WaitForExit()

let isRunningWithName processName =
    let pname = System.Diagnostics.Process.GetProcessesByName(processName)
    pname.Length <> 0
