module Util.IO

open System.IO

module File =
    let append (filePath: string) (text: string) =
        use fileStream = File.Open(filePath, FileMode.Append)
        use streamWriter = new StreamWriter(fileStream)
        streamWriter.WriteLine(text)