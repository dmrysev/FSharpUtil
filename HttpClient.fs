module Util.HttpClient

open System.Net.Http
open FSharp.Data

let initHttpClient () = 
    let handler = new HttpClientHandler(UseCookies = false)
    new HttpClient(handler)

let getContentWithHeader(httpClient: HttpClient) (headers: seq<string*string>) (url: string) =
    async {
        let message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url)
        headers |> Seq.iter message.Headers.Add
        use! response = httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
        use response = response.EnsureSuccessStatusCode()
        return! response.Content.ReadAsStringAsync() |> Async.AwaitTask
    } |> Async.RunSynchronously   

let getContent (httpClient: HttpClient) (url: string) =
    getContentWithHeader httpClient [] url

let parseHttpRequestErrorMessageStatusCode (errorMessage: string) =
    Util.Regex.matchValue @"\d+" errorMessage |> int

let parseStatusCode (httpRequestException: System.Net.Http.HttpRequestException) =
    parseHttpRequestErrorMessageStatusCode httpRequestException.Message

let loadHtml (httpClient: HttpClient) (url: string) = 
    getContent httpClient url
    |> HtmlDocument.Parse

let loadHtmlWithHeaders (httpClient: HttpClient) (headers: seq<string*string>) (url: string) =
    getContentWithHeader httpClient headers url
    |> HtmlDocument.Parse

let downloadBinaryWithHeaders (httpClient: HttpClient) (headers: seq<string*string>) (url: string) (outputFilePath: string) =
    let dirPath = System.IO.Path.GetDirectoryName(outputFilePath)
    System.IO.Directory.CreateDirectory dirPath |> ignore
    async {
        let message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url)
        headers |> Seq.iter message.Headers.Add
        use! response = httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
        use! streamToReadFrom = response.Content.ReadAsStreamAsync() |> Async.AwaitTask
        use streamToWriteTo = System.IO.File.Open(outputFilePath, System.IO.FileMode.Create)
        return! streamToReadFrom.CopyToAsync(streamToWriteTo) |> Async.AwaitTask
    } |> Async.RunSynchronously        

let downloadBinary (httpClient: HttpClient) (url: string) (outputFilePath: string) =
    downloadBinaryWithHeaders httpClient [] url outputFilePath
