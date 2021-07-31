module Util.IO.FileInfo

let getSymlinkPath path =
    let command = sprintf "readlink -f '%s'" path
    let output = Util.Process.execute command
    output.Replace("\n", "")
