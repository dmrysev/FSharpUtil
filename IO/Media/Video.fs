namespace Util.IO.Media.Video

open Util.Path

module Info =
    let duration (filePath: FilePath) = 
        $"ffprobe -i '{filePath.Value}' -show_entries format=duration -v error -of csv='p=0'"
        |> Util.Process.execute 
        |> double
        |> System.TimeSpan.FromSeconds

module Screenshot =
    let createOneAsync (inputVideoFilePath: FilePath) (timestamp: System.TimeSpan) (outputScreenshotFilePath: FilePath) = async {
        $"ffmpeg -ss {timestamp} -i '{inputVideoFilePath.Value}' -frames:v 1 -q:v 5 '{outputScreenshotFilePath.Value}' -v quiet"
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
        createManyAsync inputVideoFilePath chunksCount outputDirPath |> Async.Parallel |> Async.RunSynchronously |> ignore

module Format =
    let cutByTimestampRange (inputVideoFilePath: FilePath) (timestampRange: Util.Time.Range) (outputVideoFilePath: FilePath) =
        $"ffmpeg -ss {timestampRange.Start} -to {timestampRange.End} -i {inputVideoFilePath.Value} -c copy -v error -y {outputVideoFilePath.Value}"
        |> Util.Process.execute
        |> ignore

    let merge (temporaryDirPath: DirectoryPath) (outputVideoFilePath: FilePath) (inputVideoFilePaths: FilePath seq) =
        let videoChunksListFilePath = temporaryDirPath/FileName "list.txt"
        // Util.IO.File.create videoChunksListFilePath
        inputVideoFilePaths
        |> Seq.map (fun videoChunkFilePath -> $"file {videoChunkFilePath.Value}" )
        |> Util.IO.File.writeLines videoChunksListFilePath
        $"ffmpeg -f concat -safe 0 -i {videoChunksListFilePath.Value} -c copy -y -v error {outputVideoFilePath.Value}"
        |> Util.Process.run

    let cutByTimestampRanges (inputVideoFilePath: FilePath) (timestampRanges: Util.Time.Range seq) (temporaryDirPath: DirectoryPath) (outputVideoFilePath: FilePath) =
        let guid = Util.Guid.generate()
        let temporaryDirPath = temporaryDirPath/DirectoryName guid
        Util.IO.Directory.create temporaryDirPath
        timestampRanges
        |> Seq.mapi(fun i range ->
            let extension = inputVideoFilePath |> FilePath.fileName |> FileName.extension
            let videoChunkFileName = sprintf "%03i" i |> FileName |> FileName.setExtension extension
            let videoChunkFilePath = temporaryDirPath/videoChunkFileName
            cutByTimestampRange inputVideoFilePath range videoChunkFilePath
            videoChunkFilePath )
        |> merge temporaryDirPath outputVideoFilePath
        Util.IO.Directory.delete temporaryDirPath
