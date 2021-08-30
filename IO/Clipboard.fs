module Util.IO.Clipboard

let add (value: string) =
    let command = sprintf "echo '%s' | xclip -sel clip" value
    Util.Process.executeNoOutput(command)

let get () =
    Util.Process.execute("xclip -o -sel clip")
    |> Util.String.replace "\n" ""
