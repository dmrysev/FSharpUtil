module Util.Compression

open Util.IO.Path
open System.IO.Compression

type CompressionLevel = NoCompression | Level1 | Level3 | Level5 | Level7 | Level9

let compressionLevelToInt (compressionLevel: CompressionLevel) =
    match compressionLevel with
    | NoCompression -> 0
    | Level1 -> 1
    | Level3 -> 3 
    | Level5 -> 5 
    | Level7 -> 7 
    | Level9 -> 9

module Zip =
    let compressDirectory (inputDirPath: DirectoryPath) (outputArchiveFilePath: FilePath) (compressionLevel: CompressionLevel) =
        let compressionLevelInt = compressionLevel |> compressionLevelToInt
        Util.Process.execute $"7z a -tzip -mx={compressionLevelInt} '{outputArchiveFilePath.Value}' '{inputDirPath.Value}'" |> ignore

    let extract (inputArchive: FilePath) (outputDirPath: DirectoryPath) =
        Util.Process.execute $"7z x '{inputArchive.Value}' -o'{outputDirPath.Value}'" |> ignore
