module Util.Math.Test

open NUnit.Framework
open FsUnit

[<TestCase(1.0, 0.0)>]
[<TestCase(123.135, 0.135)>]
let ``Given float number, calling decimalPart, must return decimal part of that number`` (input, expected) =
    decimalPart input |> should (equalWithin 0.0001) expected