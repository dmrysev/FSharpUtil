module Util.HttpClient

open System.Net.Http
open FSharp.Data

let initHttpClient () = 
    let handler = new HttpClientHandler(UseCookies = false)
    new HttpClient(handler)

let getContent (httpClient: HttpClient) (url: string) =
    async {
        let message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url)
        use! response = httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
        use response = response.EnsureSuccessStatusCode()
        return! response.Content.ReadAsStringAsync() |> Async.AwaitTask
    } |> Async.RunSynchronously    

let getContentWithHeader(httpClient: HttpClient) (url: string) =
    let message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url)
    message.Headers.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64; rv:85.0) Gecko/20100101 Firefox/85.0")
    message.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8")
    message.Headers.Add("Accept-Language", "en-US,en;q=0.5")
    message.Headers.Add("Accept-Encoding", "gzip, deflate, br")
    message.Headers.Add("Connection", "keep-alive")
    message.Headers.Add("Referer", url)
    message.Headers.Add("Cookie", "__cfduid=d5ebc2523254608b56b146e26ef4a2a2a1612875434; PHPSESSID=0icjbukeungmgk634pvjkp9vlu; _ga_9TVR3DKPP6=GS1.1.1612875838.1.0.1612875838.0; _ga=GA1.1.308560825.1612875838")
    message.Headers.Add("Upgrade-Insecure-Requests", "1")
    let response = httpClient.SendAsync(message) |> Async.AwaitTask |> Async.RunSynchronously
    let response = response.EnsureSuccessStatusCode()
    response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously

let loadHtml (httpClient: HttpClient) (url: string) = 
    getContent httpClient url
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
