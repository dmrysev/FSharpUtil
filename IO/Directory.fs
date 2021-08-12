module Util.IO.Directory

open Util.IO.Path

let empty (folderPath: string) =
    System.IO.Directory.GetFileSystemEntries(folderPath).Length = 0

let countFiles (folderPath: string) =
    System.IO.Directory.EnumerateFiles folderPath |> Seq.length

let create dirPath = 
    System.IO.Directory.CreateDirectory dirPath |> ignore

let delete dirPath = 
    if Util.IO.Path.exists dirPath then
        System.IO.Directory.Delete(dirPath, true)

let generateTemporaryDirectory() =
    let tempDir = Util.Environment.SpecialFolder.temporary
    let guid = System.Guid.NewGuid().ToString()
    tempDir/guid
