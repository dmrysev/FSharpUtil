module Util.String.Test

open NUnit.Framework
open FsUnit

[<TestCase("aaa", "a", "")>]
[<TestCase("fafaaf", "a", "fff")>]
[<TestCase("fafaAf", "a", "fff")>]
[<TestCase("fafAAf", "a", "fff")>]
[<TestCase("fafAAf", "A", "fff")>]
[<TestCase("fafbfaafbbfabf", "ab", "ffffff")>]
let ``Given string and character, calling strip, must remove all characters from that string`` (inputString, characters, expected) =
    Util.String.strip characters inputString |> should equal expected