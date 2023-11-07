module Util.Web.Http

open Util.Path

let loadHtmlAsync 
    (httpClient: System.Net.Http.HttpClient) 
    (config: Util.API.Web.Http.Config) 
    (url: Url) =
    let httpErrorEvent = Event<Util.API.Web.Http.ErrorDetails>()
    let events: Util.API.Web.Http.Events = { HttpError = httpErrorEvent.Publish }
    let task = async {
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
            with 
            | :? System.Net.Http.HttpRequestException as error ->
                httpErrorEvent.Trigger { Error = error; Attempt = attempt }
                if attempt = config.MaxRetriesOnHttpError then raise error
                do! Util.Async.sleep config.HttpErrorRetryTimeout
                return tryRun(attempt + 1) |> Async.RunSynchronously }
        return tryRun(0) |> Async.RunSynchronously }
    task, events

let loadHtml httpClient config url =
    let task, events = loadHtmlAsync httpClient config url 
    task |> Async.RunSynchronously

let downloadBinaryAsync 
    (httpClient: System.Net.Http.HttpClient) 
    (config: Util.API.Web.Http.Config) 
    (url: Url) 
    (outputFilePath: FilePath) =  
    let httpErrorEvent = Event<Util.API.Web.Http.ErrorDetails>()
    let events: Util.API.Web.Http.Events = { HttpError = httpErrorEvent.Publish }
    let task = async {
        let rec tryRun(attempt: int) = async {
            try
                outputFilePath |> FilePath.directoryPath |> Util.IO.Directory.create
                let message = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url.Value)
                match config.Headers with
                | Some headers -> headers |> Seq.iter message.Headers.Add
                | None -> ()
                use! response = httpClient.SendAsync(message, System.Net.Http.HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
                response.EnsureSuccessStatusCode() |> ignore
                use! streamToReadFrom = response.Content.ReadAsStreamAsync() |> Async.AwaitTask
                use streamToWriteTo = System.IO.File.Open(outputFilePath.Value, System.IO.FileMode.Create)
                return! streamToReadFrom.CopyToAsync(streamToWriteTo) |> Async.AwaitTask             
            with 
            | :? System.Net.Http.HttpRequestException as error ->
                httpErrorEvent.Trigger { Error = error; Attempt = attempt }
                if attempt = config.MaxRetriesOnHttpError then raise error
                do! Util.Async.sleep config.HttpErrorRetryTimeout
                tryRun(attempt + 1) |> Async.RunSynchronously }
        tryRun(1) |> Async.RunSynchronously }
    task, events

let downloadBinary httpClient config url outputFilePath =
    let task, events = downloadBinaryAsync httpClient config url outputFilePath 
    task|> Async.RunSynchronously
