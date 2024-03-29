module Util.IO.Media.Ebook

open Util.Path

let createFromDirectory (inputDirectoryPath: DirectoryPath) (outputEbookFilePath: FilePath) =
    Util.Compression.Zip.compressDirectory inputDirectoryPath outputEbookFilePath Util.Compression.CompressionLevel.NoCompression
