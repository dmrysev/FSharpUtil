module Util.Test.IO.Clipboard

open NUnit.Framework
open FsUnit

[<Test>]
let ``Adding a string to clipboard and then getting it back must have same value``() =
    Util.IO.Clipboard.add("value 1")
    Util.IO.Clipboard.get() |> should equal "value 1"

[<Test>]
let ``If several strings are added to clipboard, getting from clipboard must return last added``() =
    Util.IO.Clipboard.add("value 1")
    Util.IO.Clipboard.add("value 2")
    Util.IO.Clipboard.get() |> should equal "value 2"
