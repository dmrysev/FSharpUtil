module Util.Test.IO.Path

open Util.IO.Path
open NUnit.Framework
open FsUnit

let tempDir = Util.IO.Directory.generateTemporaryDirectory()

[<SetUp>]
let setUp() =
    Util.IO.Directory.create tempDir

[<TearDown>]
let tearDown() =
    Util.IO.Directory.delete tempDir

[<Test>]
let ``Check if file or directory exists``() =
    // ARRANGE
    Util.IO.File.create (tempDir/"file_1")
    Util.IO.Directory.create (tempDir/"dir_1")

    // ACT & ASSERT
    Util.IO.Path.exists (tempDir/"file_1") |> should be True
    Util.IO.Path.exists (tempDir/"dir_1") |> should be True
    Util.IO.Path.exists (tempDir/"file_2") |> should be False
    Util.IO.Path.exists (tempDir/"dir_2") |> should be False