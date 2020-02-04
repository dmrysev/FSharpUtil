module Util.Math.Test

open NUnit.Framework
open FsUnit

[<Test>]
let ``Given float number, calling decimalPart, must return decimal part of that number`` () =
    decimalPart 123.135 |> should (equalWithin 0.0001) 0.135