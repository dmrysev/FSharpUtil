module Util.Web.Client

open Util.Path
open Util.API.Web.Client

let getHtmlContentAsync client config url =
    let task, events = Util.Web.Http.loadHtmlAsync client config url
    // events.HttpError.Add logHttpError
    task

let downloadBinaryAsync client config url outputFilePath =
    let task, events = Util.Web.Http.downloadBinaryAsync client config url outputFilePath
    // events.HttpError.Add logHttpError
    task


type Resources (torConfig: Util.API.Web.Tor.Config, webBrowserExePath: FilePath) =
    let mutable webDriver: OpenQA.Selenium.IWebDriver option = None
    let mutable torWebDriver: OpenQA.Selenium.IWebDriver option = None
    let initWebDriver() =
        let webDriverConfig: Util.Web.Driver.Config = {
            Tor = Util.API.Web.Tor.Config.Default
            WebBrowserExePath = FilePath.None
            WebDriverLocation = DirectoryPath "/usr/bin" }
        Util.Web.Driver.initChromeRemote webDriverConfig 9222
    let initTorWebDriver(torConfig: Util.API.Web.Tor.Config) =
        let webDriverConfig: Util.Web.Driver.Config = {
            Tor = torConfig
            WebBrowserExePath = FilePath.None
            WebDriverLocation = DirectoryPath "/usr/bin" }
        Util.Web.Driver.initChromeRemote webDriverConfig 9223
    member this.GetWebDriver() =
        match webDriver with
        | Some driver -> driver
        | None -> 
            webDriver <- Some (initWebDriver())
            webDriver.Value
    member this.GetTorWebDriver() =
        match torWebDriver with
        | Some driver -> driver
        | None -> 
            torWebDriver <- Some (initTorWebDriver torConfig)
            torWebDriver.Value
    interface System.IDisposable with
        member this.Dispose() = 
            match webDriver with
            | Some driver -> driver.Dispose()
            | None -> ()
            match torWebDriver with
            | Some driver -> driver.Dispose()
            | None -> ()

type TorHttpClient (torConfig: Util.API.Web.Tor.Config) =
    let client = Util.Web.Tor.initHttpClient torConfig
    interface Util.API.Web.Client.IWebClient with
        member this.GetHtmlContentAsync config url = getHtmlContentAsync client config url
        member this.GetHtmlContent config url = getHtmlContentAsync client config url |> Async.RunSynchronously
        member this.DownloadBinary config url outputFilePath = 
            downloadBinaryAsync client config url outputFilePath  |> Async.RunSynchronously
        member this.DownloadBinaryAsync config url outputFilePath = 
            downloadBinaryAsync client config url outputFilePath
    interface System.IDisposable with
        member this.Dispose() = 
            client.Dispose()

type HttpClient() =
    let client = new System.Net.Http.HttpClient()
    interface Util.API.Web.Client.IWebClient with
        member this.GetHtmlContentAsync config url = getHtmlContentAsync client config url
        member this.GetHtmlContent config url = getHtmlContentAsync client config url |> Async.RunSynchronously
        member this.DownloadBinary config url outputFilePath = 
            downloadBinaryAsync client config url outputFilePath  |> Async.RunSynchronously
        member this.DownloadBinaryAsync config url outputFilePath = 
            downloadBinaryAsync client config url outputFilePath
    interface System.IDisposable with
        member this.Dispose() = 
            client.Dispose()

type WebClient (resources: Resources) =
    let webDriver = resources.GetWebDriver()
    let httpClient = new System.Net.Http.HttpClient()
    interface Util.API.Web.Client.IWebClient with
        member this.GetHtmlContentAsync config url = async { return Util.Web.Driver.loadContent webDriver url }
        member this.GetHtmlContent config url = Util.Web.Driver.loadContent webDriver url
        member this.DownloadBinary config url outputFilePath = 
            let config = config |> Util.Web.Driver.addCookies webDriver
            downloadBinaryAsync httpClient config url outputFilePath  |> Async.RunSynchronously
        member this.DownloadBinaryAsync config url outputFilePath = 
            let config = config |> Util.Web.Driver.addCookies webDriver
            downloadBinaryAsync httpClient config url outputFilePath
    interface System.IDisposable with
        member this.Dispose() = 
            httpClient.Dispose()

type WebStreamClient() =
    let client = new System.Net.Http.HttpClient()
    let initCommand (url: Url) (outputFilePath: FilePath) = $"yt-dlp {url.Value} -o '{outputFilePath.Value}'"
    interface Util.API.Web.Client.IWebClient with
        member this.GetHtmlContentAsync config url = getHtmlContentAsync client config url
        member this.GetHtmlContent config url = getHtmlContentAsync client config url |> Async.RunSynchronously
        member this.DownloadBinary config url outputFilePath = 
            initCommand url outputFilePath
            |> Util.Process.execute
            |> ignore
        member this.DownloadBinaryAsync config url outputFilePath = async {
            initCommand url outputFilePath
            |> Util.Process.execute
            |> ignore }
            
    interface System.IDisposable with
        member this.Dispose() = 
            client.Dispose()

type TorWebClient (torConfig: Util.API.Web.Tor.Config, resources: Resources) =
    let webDriver = resources.GetTorWebDriver()
    let httpClient = Util.Web.Tor.initHttpClient torConfig
    interface Util.API.Web.Client.IWebClient with
        member this.GetHtmlContentAsync config url = async { return Util.Web.Driver.loadContent webDriver url }
        member this.GetHtmlContent config url = Util.Web.Driver.loadContent webDriver url
        member this.DownloadBinary config url outputFilePath = 
            let config = config |> Util.Web.Driver.addCookies webDriver
            downloadBinaryAsync httpClient config url outputFilePath  |> Async.RunSynchronously
        member this.DownloadBinaryAsync config url outputFilePath = 
            let config = config |> Util.Web.Driver.addCookies webDriver
            downloadBinaryAsync httpClient config url outputFilePath
    interface System.IDisposable with
        member this.Dispose() = 
            httpClient.Dispose()

let initWebClient 
    (resources: Resources) 
    (torConfig: Util.API.Web.Tor.Config) 
    (clientType: Util.API.Web.Client.ClientType) =
    match clientType with
    | HttpClient -> new HttpClient() :> Util.API.Web.Client.IWebClient
    | WebClient -> new WebClient(resources) :> Util.API.Web.Client.IWebClient
    | TorHttpClient -> new TorHttpClient (torConfig) :> Util.API.Web.Client.IWebClient
    | TorWebClient -> new TorWebClient (torConfig, resources) :> Util.API.Web.Client.IWebClient
    | WebStream -> new WebStreamClient() :> Util.API.Web.Client.IWebClient
