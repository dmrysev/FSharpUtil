open System

[<EntryPoint>]
let main argv =
    Util.Test.IO.Clipboard.``Adding to clipboard and then getting it back must be same value``()
    0
