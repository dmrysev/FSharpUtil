module Util.HttpClient

open Util.Http
open Util.IO.Path
open FSharp.Data
open System.Net.Http
open System.Threading
open System.Net.Http
open MihaZupan
open OpenQA.Selenium.Firefox

type Events = { HttpError: IEvent<HttpErrorDetails> }
and HttpErrorDetails = { Error: System.Net.Http.HttpRequestException; Attempt: int }

type HttpConfig = { 
    Headers:  seq<string * string> option
    UseTor: bool
    RetryOnHttpError: bool
    MaxRetriesOnHttpError: int
    HttpErrorRetryTimeout: System.TimeSpan }
with static member Default = {
        HttpConfig.Headers = None
        UseTor = false
        RetryOnHttpError = true
        MaxRetriesOnHttpError = 3
        HttpErrorRetryTimeout = System.TimeSpan.FromMinutes(1.0) }

let withRetryOnHttpRequestFail (delay: System.TimeSpan) (maximumAttempts: int) func onError =
    let rec tryRun(attempt: int) =
        try func()
        with 
        | :? System.Net.Http.HttpRequestException as error ->
            onError error
            if attempt = maximumAttempts then raise error
            printfn "Error %s. Attempt %i. Will try again in %A" error.Message attempt delay
            Thread.Sleep delay
            tryRun(attempt + 1)
    tryRun(1)

let loadHtmlAsync (httpClient: HttpClient) (config: HttpConfig) (url: Url) =
    let httpErrorEvent = Event<HttpErrorDetails>()
    let events = { Events.HttpError = httpErrorEvent.Publish }
    let task = async {
        let rec tryRun(attempt: int) = async {
            try
                let message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url.Value)
                match config.Headers with
                | Some headers -> headers |> Seq.iter message.Headers.Add
                | None -> ()
                use! response = httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
                use response = response.EnsureSuccessStatusCode()
                let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return content |> HtmlDocument.Parse
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

let downloadBinaryAsync (httpClient: HttpClient) (config: HttpConfig) (url: Url) (outputFilePath: FilePath) =  
    let httpErrorEvent = Event<HttpErrorDetails>()
    let events: Events = { HttpError = httpErrorEvent.Publish }
    let task = async {
        let rec tryRun(attempt: int) = async {
            try
                outputFilePath.DirectoryPath |> Util.IO.Directory.create
                let message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url.Value)
                match config.Headers with
                | Some headers -> headers |> Seq.iter message.Headers.Add
                | None -> ()
                use! response = httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
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

type TorConfig = {
    Enabled: bool
    Ip: string
    Port: int
    ControlPort: int }
with static member Default = {
        Enabled = false
        Ip = ""
        Port = 0
        ControlPort = 0 }

module TorClient =
    let init(config: TorConfig) = 
        let proxy = HttpToSocks5Proxy(config.Ip, config.Port)
        let handler = new HttpClientHandler(Proxy = proxy, UseCookies = false)
        new HttpClient(handler)

    let resetIdentity (config: TorConfig) =
        let controlPortClient = DotNetTor.ControlPort.Client(config.Ip, controlPort = config.ControlPort, password = "rookie")
        controlPortClient.ChangeCircuitAsync().Wait()

module WebDriver =
    let init(config: TorConfig) =
        let profile = FirefoxProfile()
        if config.Enabled then
            profile.SetPreference("network.proxy.type", 1)
            profile.SetPreference("network.proxy.socks", config.Ip)
            profile.SetPreference("network.proxy.socks_port", config.Port)
        profile.SetPreference("permissions.default.stylesheet", 2) // Disable CSS
        profile.SetPreference("permissions.default.image", 2) // Disable images
        profile.SetPreference("dom.ipc.plugins.enabled.libflashplayer.so", "false") // Disable Flash
        let options = FirefoxOptions(Profile = profile)
        options.AddArgument("-headless")
        new FirefoxDriver(options)

    let loadHtml (webDriver: OpenQA.Selenium.IWebDriver) (url: Url) =
        webDriver.Navigate().GoToUrl(url.Value)
        webDriver.PageSource |> HtmlDocument.Parse
