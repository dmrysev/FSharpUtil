module Util.Math.Test

open NUnit.Framework
open FsUnit

[<Test>]
let ``Given float number, calling decimalPart, must return decimal part of that number`` () =
    Assert.That(decimalPart 123.135, Is.EqualTo(0.135).Within(0.00005))