module Util.IO.Clipboard

let set (value: string) =
    let command = sprintf "echo '%s' | xclip -sel clip -r" value
    Util.Process.executeNoOutput(command)

let get () =
    let clipboardValue =
        try 
            Util.Process.execute("xclip -o -sel clip")
        with error -> 
            if error.Message.Contains "target STRING not available" then ""
            else raise error
    clipboardValue |> Util.String.removeLastCharacterIfEquals "\n"

let clear () =
    set("")
