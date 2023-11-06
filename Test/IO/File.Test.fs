module Util.Test.File

open Util.Path
open NUnit.Framework
open FsUnit

let outputDirPath = Util.IO.Directory.generateTemporaryDirectory()

[<SetUp>]
let setDown() =
    Util.IO.Directory.delete outputDirPath

// [<Test>]
// let ``Pop first line must return first line and delete it from the file``() =
//     // ARRANGE
//     let filePath = System.IO.Path.GetTempFileName()
//     System.IO.File.WriteAllLines(filePath, ["line 1"; "line 2"; "line 3"])
    
//     // ACT
//     let popedLine = Util.IO.File.popFirstLine filePath
//     let existingLines = System.IO.File.ReadAllLines filePath

//     // ASSERT
//     popedLine |> should equal "line 1"
//     existingLines |> should equal ["line 2"; "line 3"]

[<Test>]
let ``Create file must correctly dispose resources``() =
    let filePath = outputDirPath/FileName "file.txt"
    Util.IO.File.create filePath
    (fun () -> Util.IO.File.appendLine filePath "line 1") |> should not' (throw typeof<System.IO.IOException>)

