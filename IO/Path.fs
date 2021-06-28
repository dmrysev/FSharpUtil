module Util.IO.Path

let (/) path1 path2 = System.IO.Path.Combine(path1, path2)

let fixFileName (fileName: string) =
    let invalidChars = ['/'; '<'; '>'; ':'; '"'; '/'; '\\'; '|'; '?'; '*']
    Util.String.strip invalidChars fileName

let fixPath (url: string) =
    if url.EndsWith "/" then url.Remove(url.Length - 1, 1)
    else url

let exists (path: string) = 
    System.IO.Directory.Exists path || System.IO.File.Exists path

let findAvailablePathWithAppendix (filePath: string) = 
    let rec findAppendix number =
        let newFilePath = sprintf "%s_%i" filePath number
        if exists newFilePath
        then findAppendix (number + 1)
        else newFilePath
    findAppendix 1
