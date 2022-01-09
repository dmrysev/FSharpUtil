module Util.IO.Clipboard

let set (value: string) =
    let command = sprintf "echo '%s' | xclip -sel clip" value
    Util.Process.executeNoOutput(command)

let get () =
        Util.Process.execute("xclip -o -sel clip")
        |> Util.String.removeLastCharacterIfEquals "\n"

let clear () =
    set("")
