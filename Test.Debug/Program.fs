open Util
open Util.Path
open System.Security.Cryptography
open System.IO

let rawDirPath = DirectoryPath @"C:\Users\leto\tmp\raw"
let encDirPath = DirectoryPath @"C:\Users\leto\tmp\enc"
let decDirPath = DirectoryPath @"C:\Users\leto\tmp\dec"
let keyDirPath = DirectoryPath @"C:\Users\leto\tmp\key"


Util.Encryption.createKeyFile keyDirPath 
// let key = Util.Encryption.readKeyFile keyDirPath 

// Util.IO.Directory.listFiles rawDirPath
// |> Seq.iter (fun rawFilePath ->
//     let fileName = rawFilePath |> FilePath.fileName
//     let outputFilePath = encDirPath/fileName
//     Util.Encryption.encryptFile key rawFilePath outputFilePath)

// Util.IO.Directory.listFiles encDirPath
// |> Seq.iter (fun encFilePath ->
//     let fileName = encFilePath |> FilePath.fileName
//     let outputFilePath = decDirPath/fileName
//     Util.Encryption.decryptFile key encFilePath outputFilePath )
