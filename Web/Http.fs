module Util.Web.Http

open Util.Path

let loadHtmlAsync 
    (httpClient: System.Net.Http.HttpClient) 
    (config: Util.API.Web.Http.Config) 
    (url: Url) =
    let errorEvent = Event<Util.API.Web.Http.ErrorDetails>()
    let events: Util.API.Web.Http.Events = { HttpError = errorEvent.Publish }
    let rec tryRun(attempt: int) = async {
        try
            let message = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url.Value)
            match config.Headers with
            | Some headers -> headers |> Seq.iter message.Headers.Add
            | None -> ()
            use! response = httpClient.SendAsync(message, System.Net.Http.HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
            use response = response.EnsureSuccessStatusCode()
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            return content
        with error ->
            errorEvent.Trigger { Error = error; Attempt = attempt }
            if attempt = config.MaxRetriesOnError then raise error
            do! Util.Async.sleep config.ErrorRetryTimeout
            return tryRun(attempt + 1) |> Async.RunSynchronously }
    tryRun(0), events

let downloadBinaryAsync 
    (httpClient: System.Net.Http.HttpClient) 
    (config: Util.API.Web.Http.Config)
    (temporaryDirPath: DirectoryPath)
    (url: Url) 
    (outputFilePath: FilePath) =  
    let errorEvent = Event<Util.API.Web.Http.ErrorDetails>()
    let events: Util.API.Web.Http.Events = { HttpError = errorEvent.Publish }
    let tempOutputFilePath = temporaryDirPath/FileName $"{System.Guid.NewGuid().ToString()}"
    let rec tryRun(attempt: int) = async {
        try
            let message = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url.Value)
            match config.Headers with
            | Some headers -> headers |> Seq.iter message.Headers.Add
            | None -> ()
            use! response = httpClient.SendAsync(message, System.Net.Http.HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
            response.EnsureSuccessStatusCode() |> ignore
            use! inputStream = response.Content.ReadAsStreamAsync() |> Async.AwaitTask
            use outputStream = System.IO.File.Open(tempOutputFilePath.Value, System.IO.FileMode.Create)
            do! inputStream.CopyToAsync(outputStream) |> Async.AwaitTask
            outputFilePath |> FilePath.directoryPath |> Util.IO.Directory.ensureExists
            Util.IO.File.move tempOutputFilePath outputFilePath
        with error ->
            errorEvent.Trigger { Error = error; Attempt = attempt }
            if attempt = config.MaxRetriesOnError then 
                Util.IO.File.delete tempOutputFilePath
                raise error
            else 
                do! Util.Async.sleep config.ErrorRetryTimeout
                tryRun(attempt + 1) |> Async.RunSynchronously }
    tryRun(0), events
