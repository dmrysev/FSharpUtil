open Util
open Util.IO.Path

let filePath = FilePath "/mnt/data/anime/Castlevania/Castlevania.S04.WEB-DL.1080p.Rus.Eng/Castlevania.S04E01.WEBDL.1080p.AF.RGzsRutracker.mkv"
let outputDirPath = DirectoryPath "/mnt/data/tmp/test2"
Util.IO.Directory.listFiles outputDirPath
|> Seq.iter Util.IO.File.delete
Util.IO.Media.Video.Screenshot.createMany filePath 10 outputDirPath
