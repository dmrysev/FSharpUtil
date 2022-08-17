module Util.Test.Json

open Util
open Util.IO.Path
open NUnit.Framework
open FsUnit

type SimpleMessage = {
    StringField: string
    IntField: int }

type UnionMessage = ChoiceA | ChoiceB

type ComplexUnionMessage = ChoiceA of string | ChoiceB of int

type OptionTypeMessage = {
    StringField: string option }

type ComplexTypeMessage = {
    Path: FilePath }

[<Test>]
let ``Json serialization must support simple types``() =
    // ARRANGE
    let testMessage: SimpleMessage = {
        StringField = "test string"
        IntField = 100 }
    // ACT
    let jsonString = testMessage |> Json.toJson
    let resultMessage = Json.fromJson<SimpleMessage> jsonString

    // ASSERT
    resultMessage |> should equal testMessage

[<Test>]
let ``Json serialization must support union types``() =
    // ARRANGE
    let testMessage = UnionMessage.ChoiceA
    // ACT
    let jsonString = testMessage |> Json.toJson
    let resultMessage = Json.fromJson<UnionMessage> jsonString

    // ASSERT
    resultMessage |> should equal testMessage

[<Test>]
let ``Json serialization must support complex union types``() =
    // ARRANGE
    let testMessageA = ComplexUnionMessage.ChoiceA "test string"
    let testMessageB = ComplexUnionMessage.ChoiceB 100

    // ACT
    let jsonStringA = testMessageA |> Json.toJson
    let resultMessageA = Json.fromJson<ComplexUnionMessage> jsonStringA
    let jsonStringB = testMessageB |> Json.toJson
    let resultMessageB = Json.fromJson<ComplexUnionMessage> jsonStringB

    // ASSERT
    resultMessageA |> should equal testMessageA
    resultMessageB |> should equal testMessageB

[<Test>]
let ``Json serialization must support option types``() =
    // ARRANGE
    let testMessageA: OptionTypeMessage = { StringField = Some "test string" }
    let testMessageB: OptionTypeMessage = { StringField = None }

    // ACT
    let jsonStringA = testMessageA |> Json.toJson
    let resultMessageA = Json.fromJson<OptionTypeMessage> jsonStringA
    let jsonStringB = testMessageB |> Json.toJson
    let resultMessageB = Json.fromJson<OptionTypeMessage> jsonStringB

    // ASSERT
    resultMessageA |> should equal testMessageA
    resultMessageB |> should equal testMessageB

[<Test>]
let ``Json serialization must support complex types``() =
    // ARRANGE
    let testMessage: ComplexTypeMessage = { Path = FilePath "/some/path/file.jpg" }
    // ACT
    let jsonString = testMessage |> Json.toJson
    let resultMessage = Json.fromJson<ComplexTypeMessage> jsonString

    // ASSERT
    resultMessage.Path.Value |> should equal testMessage.Path.Value
    resultMessage |> should equal testMessage

