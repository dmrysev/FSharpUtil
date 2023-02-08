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
