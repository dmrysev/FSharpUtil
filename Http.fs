module Util.Http

open FSharp.Data
open System.IO

let downloadBinaryFile (url: string) (outputFilePath: string) =
    let dirPath = Path.GetDirectoryName(outputFilePath)
    if not <| (Directory.Exists dirPath) then Directory.CreateDirectory dirPath |> ignore
    match Http.Request(url).Body with
    | Text text -> failwith (sprintf "Could not download binary.\nServer returned\n%s" text)
    | Binary bytes -> File.WriteAllBytes(outputFilePath, bytes)

let downloadBinaryFileAsync url output = async {
    let! response = Http.AsyncRequest(url)
    match response.Body with
    | Text text -> failwith (sprintf "Could not download binary.\nServer returned\n%s" text)
    | Binary bytes -> File.WriteAllBytes(output, bytes) }