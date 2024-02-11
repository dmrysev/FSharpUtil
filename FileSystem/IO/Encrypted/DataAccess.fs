module Util.IO.FileSystem.Encrypted.DataAccess

open Util
open Util.Path

let init (appDataDirPath: DirectoryPath) =
    let appDataDirPath = Util.IO.Directory.initialize (appDataDirPath/DirectoryName "FileSystem")
    let trashBinDirPath = Util.IO.Directory.initialize (appDataDirPath/DirectoryName "trash")
    let keyLocation = Util.IO.Directory.initialize (appDataDirPath/DirectoryName "key")
    if Util.IO.Directory.exists keyLocation |> not then 
        Util.Encryption.createKeyFile keyLocation
    let key = Util.Encryption.readKeyFile keyLocation
    let defaultDataAccess = IO.FileSystem.Default.DataAccess.init appDataDirPath
    let dataAccess = { 
        defaultDataAccess with
            File = {| 
                defaultDataAccess.File with
                    ReadBytes = Util.Encryption.decryptFileToBytes key |}     }
    dataAccess
