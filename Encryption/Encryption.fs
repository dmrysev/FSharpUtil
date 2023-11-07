module Util.Encryption

open Util.Path
open NETCore.Encrypt

type Key = { Key: string; IV: string }

let createKeyFile (keyDirPath: DirectoryPath) =
    let aesKey  = EncryptProvider.CreateAesKey()
    let keyFilePath = keyDirPath/FileName "key"
    let ivFilePath = keyDirPath/FileName "iv"
    Util.IO.File.writeText keyFilePath aesKey.Key
    Util.IO.File.writeText ivFilePath aesKey.IV

let readKeyFile (keyDirPath: DirectoryPath) =
    let keyFilePath = keyDirPath/FileName "key"
    let ivFilePath = keyDirPath/FileName "iv"
    { Key = Util.IO.File.readAllText keyFilePath; IV = Util.IO.File.readAllText ivFilePath }

let createPasswordProtectedKeyFile (keyDirPath: DirectoryPath) (password: string) =
    let aesKey  = EncryptProvider.CreateAesKey()
    let encKey = EncryptProvider.AESEncrypt(aesKey.Key, password)
    let encIV = EncryptProvider.AESEncrypt(aesKey.IV, password)
    let keyFilePath = keyDirPath/FileName "key"
    let ivFilePath = keyDirPath/FileName "iv"
    Util.IO.File.writeText keyFilePath aesKey.Key
    Util.IO.File.writeText ivFilePath aesKey.IV

let readPasswordProtectedKeyFile (keyDirPath: DirectoryPath) (password: string) =
    let keyFilePath = keyDirPath/FileName "key"
    let ivFilePath = keyDirPath/FileName "iv"
    let encKey = Util.IO.File.readAllText keyFilePath
    let encIV = Util.IO.File.readAllText ivFilePath
    let key = EncryptProvider.AESDecrypt(encKey, password)
    let iv = EncryptProvider.AESDecrypt(encIV, password)
    { Key = key; IV = iv }

let encryptFile (key: Key) (inputFilePath: FilePath) (outputFilePath: FilePath) =
    let srcBytes = Util.IO.File.readBytes inputFilePath
    let encryptedBytes = EncryptProvider.AESEncrypt(srcBytes, key.Key, key.IV)
    Util.IO.File.writeBytes outputFilePath encryptedBytes

let decryptFileToBytes (key: Key) (inputFilePath: FilePath) =
    let encryptedBytes = Util.IO.File.readBytes inputFilePath
    EncryptProvider.AESDecrypt(encryptedBytes, key.Key, key.IV)

let decryptFile (key: Key) (inputFilePath: FilePath) (outputFilePath: FilePath) =
    decryptFileToBytes key inputFilePath
    |> Util.IO.File.writeBytes outputFilePath
