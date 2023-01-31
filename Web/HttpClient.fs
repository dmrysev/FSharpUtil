module Util.Web.HttpClient

open Util.IO.Path
open Util.API.Web
open FSharp.Data
open System.Net.Http
open System.Threading
open MihaZupan
open OpenQA.Selenium.Firefox

let getHtmlContentAsync (httpClient: HttpClient) (config: HttpConfig) (url: Url) =
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
                return content
            with 
            | :? System.Net.Http.HttpRequestException as error ->
                httpErrorEvent.Trigger { Error = error; Attempt = attempt }
                if attempt = config.MaxRetriesOnHttpError then raise error
                do! Util.Async.sleep config.HttpErrorRetryTimeout
                return tryRun(attempt + 1) |> Async.RunSynchronously }
        return tryRun(0) |> Async.RunSynchronously }
    task, events

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
                outputFilePath |> FilePath.directoryPath |> Util.IO.Directory.create
                let message = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url.Value)
                match config.Headers with
                | Some headers -> headers |> Seq.iter message.Headers.Add
                | None -> ()
                use! response = httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask
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

module TorClient =
    let init(config: TorConfig) = 
        let proxy = HttpToSocks5Proxy(config.Ip, config.Port)
        let handler = new HttpClientHandler(Proxy = proxy, UseCookies = false)
        new HttpClient(handler)

    let resetIdentity (config: TorConfig) =
        let controlPortClient = DotNetTor.ControlPort.Client(config.Ip, controlPort = config.ControlPort, password = "rookie")
        controlPortClient.ChangeCircuitAsync().Wait()

module WebDriver =
    type Config = {
        Tor: TorConfig
        WebBrowserExePath: FilePath
        WebDriverLocation: DirectoryPath }

    let initProxySettings (torConfig: TorConfig) =
        let proxy = OpenQA.Selenium.Proxy()
        proxy.SocksProxy <- $"{torConfig.Ip}:{torConfig.Port}"
        proxy.SocksVersion <- 5
        proxy
        
    let initChrome(config: Config) =
        let options = OpenQA.Selenium.Chrome.ChromeOptions()
        if config.Tor.Enabled then options.Proxy <- initProxySettings config.Tor
        new OpenQA.Selenium.Chrome.ChromeDriver(options) :> OpenQA.Selenium.IWebDriver

    let initChromeRemote(config: Config) (port: int) =
        let options = OpenQA.Selenium.Chrome.ChromeOptions()
        options.DebuggerAddress <- $"127.0.0.1:{port}"
        if config.Tor.Enabled then options.Proxy <- initProxySettings config.Tor
        new OpenQA.Selenium.Chrome.ChromeDriver(config.WebDriverLocation.Value, options) :> OpenQA.Selenium.IWebDriver

    let initFirefox(config: Config) =
        let options = OpenQA.Selenium.Firefox.FirefoxOptions()
        if config.Tor.Enabled then options.Proxy <- initProxySettings config.Tor
        new OpenQA.Selenium.Firefox.FirefoxDriver(options) :> OpenQA.Selenium.IWebDriver

    let loadContent (webDriver: OpenQA.Selenium.IWebDriver) (url: Url) =
        webDriver.Navigate().GoToUrl(url.Value)
        webDriver.PageSource

    let loadHtml (webDriver: OpenQA.Selenium.IWebDriver) (url: Url) =
        loadContent webDriver url |> HtmlDocument.Parse

