namespace Util.IO.Media.Video

open Util.IO.Path

module Info =
    let duration (filePath: FilePath) = 
        $"ffprobe -i '{filePath.Value}' -show_entries format=duration -v error -of csv='p=0'"
        |> Util.Process.execute 
        |> double
        |> System.TimeSpan.FromSeconds

module Screenshot =
    let createOneAsync (inputVideoFilePath: FilePath) (timestamp: System.TimeSpan) (outputScreenshotFilePath: FilePath) = async {
        $"ffmpeg -ss {timestamp} -i '{inputVideoFilePath.Value}' -frames:v 1 -q:v 5 '{outputScreenshotFilePath.Value}' -v error"
        |> Util.Process.execute
        |> ignore }

    let createOne (inputVideoFilePath: FilePath) (timestamp: System.TimeSpan) (outputScreenshotFilePath: FilePath) = 
        createOneAsync inputVideoFilePath timestamp outputScreenshotFilePath |> Async.RunSynchronously

    let createManyAsync (inputVideoFilePath: FilePath) (chunksCount: int) (outputDirPath: DirectoryPath) = 
        let duration = Info.duration inputVideoFilePath
        let durationChunk = duration/(double chunksCount)
        seq { for i in 0 .. (chunksCount - 1) -> (double i) * durationChunk }
        |> Seq.mapi (fun i timestamp -> async {
            let fileName = sprintf "%03i.jpg" (i + 1) |> FileName
            let outputScreenshotPath = outputDirPath/fileName
            do! createOneAsync inputVideoFilePath timestamp outputScreenshotPath }) 
        
    let createMany (inputVideoFilePath: FilePath) (chunksCount: int) (outputDirPath: DirectoryPath) =
        createManyAsync inputVideoFilePath chunksCount outputDirPath |> Async.Parallel |> Async.RunSynchronously
