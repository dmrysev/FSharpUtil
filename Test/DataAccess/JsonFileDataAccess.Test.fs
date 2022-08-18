module Util.Test.JsonFileDataAccess

open Util
open Util.IO.Path
open NUnit.Framework
open FsUnit

type Message = {
    Text: string }

let outputDirPath = Util.IO.Directory.generateTemporaryDirectory()
let initJsonFileDataAccess() = 
    Util.DataAccess.JsonFileDataAccess(outputDirPath)

[<SetUp>]
let setUp() =
    Util.IO.Directory.delete outputDirPath

[<TearDown>]
let tearDown() =
    Util.IO.Directory.delete outputDirPath

let writeMessage (dataAccess: Util.DataAccess.JsonFileDataAccess) (id: string) message =
    System.Threading.Thread.Sleep 15
    let jsonString = message |> Util.Json.toJson
    dataAccess.Write id jsonString

[<Test>]
let ``Writing messages to json file data access and then reading them back, must return exactly same messages preserving order``() =
    // ARRANGE
    let dataAccess = initJsonFileDataAccess()
    let writeMessage = writeMessage dataAccess
    writeMessage"id_1" { Text = "test 1" }
    writeMessage "id_2" { Text = "test 2" }
    writeMessage "id_3" { Text = "test 3" }

    // ACT
    let result = dataAccess.ReadAll() |> Seq.map Util.Json.fromJson<Message>

    // ASSERT
    result |> Seq.length |> should equal 3
    result |> should equal [| { Text = "test 1" }; { Text = "test 2" }; { Text = "test 3" } |]
