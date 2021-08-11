module Util.Test.File

open NUnit.Framework
open FsUnit

[<Test>]
let ``Pop first line must return first line and delete it from the file``() =
    // ARRANGE
    let filePath = System.IO.Path.GetTempFileName()
    printfn "%s" filePath
    System.IO.File.WriteAllLines(filePath, ["line 1"; "line 2"; "line 3"])
    
    // ACT
    let popedLine = Util.IO.File.popFirstLine filePath
    let existingLines = System.IO.File.ReadAllLines filePath

    // ASSERT
    popedLine |> should equal "line 1"
    existingLines |> should equal ["line 2"; "line 3"]
