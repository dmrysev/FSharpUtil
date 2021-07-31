module Util.IO.Directory

let empty (folderPath: string) =
    System.IO.Directory.GetFileSystemEntries(folderPath).Length = 0

let countFiles (folderPath: string) =
    System.IO.Directory.EnumerateFiles folderPath |> Seq.length

let create dirPath = 
    System.IO.Directory.CreateDirectory dirPath |> ignore
