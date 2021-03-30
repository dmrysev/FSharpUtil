module Util.VersionControl

let init(dirPath: string) =
    let proc = Util.Process.run "git init"
    proc.WaitForExit()

let add(filePath: string) = 
    let command = sprintf "git add %s" filePath
    let proc = Util.Process.run command
    proc.WaitForExit()

let commit(message: string) = 
    let command = sprintf "git commit -m '%s'" message
    let proc = Util.Process.run command
    proc.WaitForExit()