module Util.Test.IO.Clipboard

open NUnit.Framework
open FsUnit

[<TestCase("value 1")>]
[<TestCase("value 1\nvalue 2")>]
[<TestCase("value 1\nvalue 2\n")>]
let ``If a value is set to clipboard, getting from clipboard, must return same value``(value: string) =
    Util.IO.Clipboard.set value
    Util.IO.Clipboard.get() |> should equal value

[<Test>]
let ``If several values are set to clipboard, getting from clipboard, must return last added``() =
    Util.IO.Clipboard.set("value 1")
    Util.IO.Clipboard.set("value 2")
    Util.IO.Clipboard.get() |> should equal "value 2"

[<Test>]
let ``If clipboard is cleared, getting from clipboard, must return empty string``() =
    Util.IO.Clipboard.clear()
    Util.IO.Clipboard.get() |> should equal ""

[<Test>]
let ``If a value added to clipboard, calling get multiple time, must return same value all the time()`` =
    Util.IO.Clipboard.set("value 1")
    Util.IO.Clipboard.get() |> should equal "value 1"
    Util.IO.Clipboard.get() |> should equal "value 1"
