module Util.HttpClient

open Util.Http
open Util.IO.Path
open System.Net.Http
open FSharp.Data
open System.Threading

let withRetryOnHttpRequestFail (delay: System.TimeSpan) (maximumAttempts: int) func =
    let rec tryRun(attempt: int) =
        try func()
        with 
        | :? System.Net.Http.HttpRequestException as error ->
            if attempt = maximumAttempts then raise error
            printfn "Error %s. Attempt %i. Will try again in %A" error.Message attempt delay
            Thread.Sleep delay
            tryRun(attempt + 1)
    tryRun(1)

let initHttpClient () = 
    let handler = new HttpClientHandler(UseCookies = false)
    new HttpClient(handler)

let getContentWithHeader(httpClient: HttpClient) (headers: seq<string*string>) (url: Url) =
    async {
        let message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url.Value)
        headers |> Seq.iter message.Headers.Add
        use! response = httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
        use response = response.EnsureSuccessStatusCode()
        return! response.Content.ReadAsStringAsync() |> Async.AwaitTask
    } |> Async.RunSynchronously   

let getContent (httpClient: HttpClient) (url: Url) =
    getContentWithHeader httpClient [] url

let parseHttpRequestErrorMessageStatusCode (errorMessage: string) =
    Util.Regex.matchValue @"\d+" errorMessage |> int

let parseStatusCode (httpRequestException: System.Net.Http.HttpRequestException) =
    parseHttpRequestErrorMessageStatusCode httpRequestException.Message

let loadHtml (httpClient: HttpClient) (url: Url) = 
    getContent httpClient url
    |> HtmlDocument.Parse

let loadHtmlWithHeaders (httpClient: HttpClient) (headers: seq<string*string>) (url: Url) =
    getContentWithHeader httpClient headers url
    |> HtmlDocument.Parse

let downloadBinaryWithHeaders (httpClient: HttpClient) (headers: seq<string*string>) (url: Url) (outputFilePath: FilePath) =
    outputFilePath.DirectoryPath
    |> Util.IO.Directory.create
    async {
        let message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url.Value)
        headers |> Seq.iter message.Headers.Add
        use! response = httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
        use! streamToReadFrom = response.Content.ReadAsStreamAsync() |> Async.AwaitTask
        use streamToWriteTo = System.IO.File.Open(outputFilePath.Value, System.IO.FileMode.Create)
        return! streamToReadFrom.CopyToAsync(streamToWriteTo) |> Async.AwaitTask
    } |> Async.RunSynchronously

let downloadBinary (httpClient: HttpClient) (url: Url) (outputFilePath: FilePath) =
    downloadBinaryWithHeaders httpClient [] url outputFilePath
