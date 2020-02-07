module Util.String.Test

open NUnit.Framework
open FsUnit

[<TestCase("fafaf", [|'a'|], "fff")>]
let ``Given string and character, calling strip, must remove all characters from that string`` (inputString, characters, expected) =
    Util.String.strip characters inputString |> should equal expected