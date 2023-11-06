module Util.Test.Json

open Util
open Util.Path
open Util.Json
open NUnit.Framework
open FsUnit
open System

type SimpleMessage = { StringField: string; IntField: int }
type UnionMessage = ChoiceA | ChoiceB
type SimpleTypesUnionMessage = ChoiceA of string | ChoiceB of int
type OptionTypeMessage = { StringField: string option }
type ComplexTypeMessage = { Path: FilePath; Timestamp: DateTime }
with static member Default = { Path = FilePath "/some/path/file.jpg"; Timestamp = DateTime(2010, 8, 18, 16, 32, 0) }
type ComplexTypesUnionMessage = ChoiceA of SimpleMessage | ChoiceB of ComplexTypeMessage
type DictTypeMessage = { Dict: Collections.IDictionary } 
with static member Default = { Dict = Collections.Generic.Dictionary<string,obj>() }

[<Test>]
let ``Json serialization must support simple types``() =
    // ARRANGE
    let testMessage: SimpleMessage = { StringField = "test string"; IntField = 100 }
        
    // ACT
    let resultMessage = testMessage |> Json.toJson |> Json.fromJson<SimpleMessage>

    // ASSERT
    resultMessage |> should equal testMessage

[<Test>]
let ``Json serialization must support union types``() =
    // ARRANGE
    let testMessage = UnionMessage.ChoiceA

    // ACT
    let resultMessage = testMessage |> Json.toJson |> Json.fromJson<UnionMessage>

    // ASSERT
    resultMessage |> should equal testMessage

[<Test>]
let ``Json serialization must support union of simple types``() =
    // ARRANGE
    let testMessageA = SimpleTypesUnionMessage.ChoiceA "test string"
    let testMessageB = SimpleTypesUnionMessage.ChoiceB 100

    // ACT
    let resultMessageA = testMessageA |> Json.toJson |> Json.fromJson<SimpleTypesUnionMessage>
    let resultMessageB = testMessageB |> Json.toJson |> Json.fromJson<SimpleTypesUnionMessage>

    // ASSERT
    resultMessageA |> should equal testMessageA
    resultMessageB |> should equal testMessageB

[<Test>]
let ``Json serialization must support option types``() =
    // ARRANGE
    let testMessageA: OptionTypeMessage = { StringField = Some "test string" }
    let testMessageB: OptionTypeMessage = { StringField = None }

    // ACT
    let resultMessageA = testMessageA |> Json.toJson |> Json.fromJson<OptionTypeMessage>
    let resultMessageB = testMessageB |> Json.toJson |> Json.fromJson<OptionTypeMessage>

    // ASSERT
    resultMessageA |> should equal testMessageA
    resultMessageB |> should equal testMessageB

[<Test>]
let ``Json serialization must support complex types``() =
    // ARRANGE
    let testMessage = ComplexTypeMessage.Default

    // ACT
    let jsonString = testMessage |> Json.toJson
    let resultMessage = Json.fromJson<ComplexTypeMessage> jsonString

    // ASSERT
    resultMessage.Path.Value |> should equal testMessage.Path.Value
    resultMessage |> should equal testMessage

[<Test>]
let ``Json serialization must support union of complex types``() =
    // ARRANGE
    let testMessageA = ComplexTypesUnionMessage.ChoiceA { StringField = "test string"; IntField = 100 }
    let testMessageB = ComplexTypesUnionMessage.ChoiceB ComplexTypeMessage.Default

    // ACT
    let resultMessageA = testMessageA |> Json.toJson |> Json.fromJson<ComplexTypesUnionMessage>
    let resultMessageB = testMessageB |> Json.toJson |> Json.fromJson<ComplexTypesUnionMessage>

    // ASSERT
    resultMessageA |> should equal testMessageA
    resultMessageB |> should equal testMessageB

[<Test>]
let ``Json serialization must support byte array``() =
    // ARRANGE
    // let testMessage = "test byte array string"
    // let testBytes = System.Convert.FromBase64String(testMessage)
    let testBytes: byte array = [| byte(0x00); byte(0x21); byte(0x60) |]

    // ACT
    let resultBytes = testBytes |> Json.toJson |> Json.fromJson<byte array>

    // ASSERT
    resultBytes |> should equal testBytes

[<Test>]
let ``Json serialization must support dictionary of string key and obj value``() =
    // ARRANGE
    let testMessage = DictTypeMessage.Default
    testMessage.Dict["Key1"] <- "Value1"
    testMessage.Dict["Key2"] <- 33
    testMessage.Dict["Key3"] <- FilePath "/some/path/file.jpg"
    testMessage.Dict["Key4"] <- Url "https://somewhere.com"

    // ACT
    let jsonString = testMessage |> Json.toJson
    let resultMessage = Json.fromJson<DictTypeMessage> jsonString

    // ASSERT
    resultMessage.Dict["Key1"] |> should equal testMessage.Dict["Key1"]
    resultMessage.Dict["Key2"] |> should equal testMessage.Dict["Key2"]
    resultMessage.Dict["Key3"] |> FilePath.parseJsonObj |> should equal testMessage.Dict["Key3"]
    resultMessage.Dict["Key4"] |> Url.parseJsonObj |> should equal testMessage.Dict["Key4"]