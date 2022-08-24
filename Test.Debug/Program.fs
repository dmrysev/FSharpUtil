open Util
open Util.IO.Path
open Util.IO.Media.Video.Format

let filePath = FilePath "/mnt/data/tmp/test/SpankbangComVideorgg6.mp4"
let outputFilePath = FilePath "/mnt/data/tmp/test/merged.mp4"
let tempDirPath = DirectoryPath "/mnt/data/tmp/test/temp"
let timestampRanges = [ 
    { Start = System.TimeSpan.FromMinutes(1); End = System.TimeSpan.FromMinutes(2) }
    { Start = System.TimeSpan.FromMinutes(5); End = System.TimeSpan.FromMinutes(7) }
    { Start = System.TimeSpan.FromMinutes(8); End = System.TimeSpan.FromMinutes(9) }
]
Util.IO.Media.Video.Format.cutByTimestampRanges filePath timestampRanges tempDirPath outputFilePath 
