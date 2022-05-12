module Util.Json

open Util.IO.Path
open Newtonsoft.Json

let toJson message = JsonConvert.SerializeObject message
let fromJson<'a> json = JsonConvert.DeserializeObject<'a> json
let parse text = Linq.JObject.Parse text
let read (jsonFilePath: FilePath) = Util.IO.File.readAllText jsonFilePath |> parse
let deserializeFile<'a> (jsonFilePath: FilePath) = Util.IO.File.readAllText jsonFilePath |> fromJson<'a>
let serializeToFile (jsonFilePath: FilePath)  (message: 'a) = message |> toJson |> Util.IO.File.writeText jsonFilePath
