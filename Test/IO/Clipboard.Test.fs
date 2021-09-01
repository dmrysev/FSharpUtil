module Util.Test.IO.Clipboard

open NUnit.Framework
open FsUnit

[<TestCase("value 1")>]
[<TestCase("value 1\nvalue 2")>]
[<TestCase("value 1\nvalue 2\n")>]
let ``Adding a string to clipboard and then getting it back must have same value``(value: string) =
    Util.IO.Clipboard.add value
    Util.IO.Clipboard.get() |> should equal value

[<Test>]
let ``If several strings are added to clipboard, getting from clipboard must return last added``() =
    Util.IO.Clipboard.add("value 1")
    Util.IO.Clipboard.add("value 2")
    Util.IO.Clipboard.get() |> should equal "value 2"
