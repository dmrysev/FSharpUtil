module Util.IO.Clipboard

let set (value: string) =
    let command = sprintf "echo '%s' | xclip -sel clip" value
    Util.Process.executeNoOutput(command)

let get () =
    let clipboardString = Util.Process.execute("xclip -o -sel clip")
    if clipboardString.EndsWith("\n") then clipboardString |> Util.String.removeLastCharacter 1
    else clipboardString

let clear () =
    set("")
