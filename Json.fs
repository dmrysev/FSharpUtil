module Util.Json

open Util.Path
open Newtonsoft.Json

let defaultSettings =
    let settings = JsonSerializerSettings()
    settings.ReferenceLoopHandling <- ReferenceLoopHandling.Error
    settings
let toJson message = JsonConvert.SerializeObject(message, defaultSettings)
let toJsonIndented message = JsonConvert.SerializeObject(message, Formatting.Indented, defaultSettings)
let fromJson<'a> json = JsonConvert.DeserializeObject<'a>(json, defaultSettings)
let parse text = Linq.JObject.Parse text
let read (jsonFilePath: FilePath) = Util.IO.File.readAllText jsonFilePath |> parse
let deserializeFile<'a> (jsonFilePath: FilePath) = Util.IO.File.readAllText jsonFilePath |> fromJson<'a>
let serializeToFile (jsonFilePath: FilePath)  (message: 'a) = message |> toJson |> Util.IO.File.writeText jsonFilePath
