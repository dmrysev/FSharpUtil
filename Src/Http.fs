module Util.Http

open FSharp.Data
open System.IO

let downloadBinaryFile url outputFilePath =
    match Http.Request(url).Body with
    | Text _ -> failwith "Could not download binary"
    | Binary bytes -> File.WriteAllBytes(outputFilePath, bytes)