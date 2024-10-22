module Util.Web.Client

open Util
open Util.Path
open Util.API.Web.Client

let getHtmlContentAsync client config (httpError: Event<API.Web.Http.ErrorDetails>) url =
    let task, events = Util.Web.Http.loadHtmlAsync client config url
    events.HttpError.Add httpError.Trigger
    task

let downloadBinaryAsync client config temporaryDirPath (httpError: Event<API.Web.Http.ErrorDetails>) url outputFilePath =
    let task, events = Util.Web.Http.downloadBinaryAsync client config temporaryDirPath url outputFilePath
    events.HttpError.Add httpError.Trigger
    task

let initProxyHttpClient (proxy: Util.API.Web.Http.Proxy) = 
    let proxy = MihaZupan.HttpToSocks5Proxy (proxy.Ip, proxy.Port)
    let handler = new System.Net.Http.HttpClientHandler (Proxy = proxy, UseCookies = true)
    new System.Net.Http.HttpClient (handler)

type Resources (
    webDriverLocation: DirectoryPath, 
    proxy: Util.API.Web.Http.Proxy,
    remoteDebuggerPort: int,
    remoteDebuggerPortProxy: int) =
    let mutable webDriver: OpenQA.Selenium.IWebDriver option = None
    let mutable proxyWebDriver: OpenQA.Selenium.IWebDriver option = None
    let initWebDriver() =
        let webDriverConfig = {
            Util.Web.Driver.Config.Default with
                WebDriverLocation = webDriverLocation }
        Util.Web.Driver.initChromeRemote webDriverConfig remoteDebuggerPort
    let initProxyWebDriver(proxy: Util.API.Web.Http.Proxy) =
        let webDriverConfig = {
            Util.Web.Driver.Config.Default with
                Proxy = Some proxy
                WebDriverLocation = webDriverLocation }
        Util.Web.Driver.initChromeRemote webDriverConfig remoteDebuggerPortProxy
    member this.GetWebDriver() =
        match webDriver with
        | Some driver -> driver
        | None -> 
            webDriver <- Some (initWebDriver())
            webDriver.Value
    member this.GetProxyWebDriver() =
        match proxyWebDriver with
        | Some driver -> driver
        | None -> 
            proxyWebDriver <- Some (initProxyWebDriver proxy)
            proxyWebDriver.Value
    interface System.IDisposable with
        member this.Dispose() = 
            match webDriver with
            | Some driver -> driver.Dispose()
            | None -> ()
            match proxyWebDriver with
            | Some driver -> driver.Dispose()
            | None -> ()

type ProxyHttpClient (proxy: Util.API.Web.Http.Proxy, temporaryDirPath: DirectoryPath) =
    let client = initProxyHttpClient proxy
    let httpError = Event<API.Web.Http.ErrorDetails>()
    interface Util.API.Web.Client.IWebClient with
        member this.GetHtmlContentAsync config url = getHtmlContentAsync client config httpError url
        member this.GetHtmlContent config url = getHtmlContentAsync client config httpError url |> Async.RunSynchronously
        member this.DownloadBinary config url outputFilePath = 
            downloadBinaryAsync client config temporaryDirPath httpError url outputFilePath  |> Async.RunSynchronously
        member this.DownloadBinaryAsync config url outputFilePath = 
            downloadBinaryAsync client config temporaryDirPath httpError url outputFilePath
        member this.Error = httpError.Publish
    interface System.IDisposable with
        member this.Dispose() = 
            client.Dispose()

type HttpClient (temporaryDirPath: DirectoryPath) =
    let client = new System.Net.Http.HttpClient()
    let httpError = Event<API.Web.Http.ErrorDetails>()
    interface Util.API.Web.Client.IWebClient with
        member this.GetHtmlContentAsync config url = getHtmlContentAsync client config httpError url
        member this.GetHtmlContent config url = getHtmlContentAsync client config httpError url |> Async.RunSynchronously
        member this.DownloadBinary config url outputFilePath = 
            downloadBinaryAsync client config temporaryDirPath httpError url outputFilePath  |> Async.RunSynchronously
        member this.DownloadBinaryAsync config url outputFilePath = 
            downloadBinaryAsync client config temporaryDirPath httpError url outputFilePath
        member this.Error = httpError.Publish
    interface System.IDisposable with
        member this.Dispose() = 
            client.Dispose()

type WebClient (resources: Resources, temporaryDirPath: DirectoryPath) =
    let webDriver = resources.GetWebDriver()
    let httpClient = new System.Net.Http.HttpClient()
    let httpError = Event<API.Web.Http.ErrorDetails>()
    interface Util.API.Web.Client.IWebClient with
        member this.GetHtmlContentAsync config url = async { return Util.Web.Driver.loadContent webDriver url }
        member this.GetHtmlContent config url = Util.Web.Driver.loadContent webDriver url
        member this.DownloadBinary config url outputFilePath = 
            let config = config |> Util.Web.Driver.addCookies webDriver
            downloadBinaryAsync httpClient config temporaryDirPath httpError url outputFilePath  |> Async.RunSynchronously
        member this.DownloadBinaryAsync config url outputFilePath = 
            let config = config |> Util.Web.Driver.addCookies webDriver
            downloadBinaryAsync httpClient config temporaryDirPath httpError url outputFilePath
        member this.Error = httpError.Publish
    interface System.IDisposable with
        member this.Dispose() = 
            httpClient.Dispose()

type WebStreamClient (temporaryDirPath: DirectoryPath) =
    let client = new System.Net.Http.HttpClient()
    let initCommand (url: Url) (outputFilePath: FilePath) = $"yt-dlp {url.Value} -o '{outputFilePath.Value}' -S 'res:720'"
    let httpError = Event<API.Web.Http.ErrorDetails>()
    interface Util.API.Web.Client.IWebClient with
        member this.GetHtmlContentAsync config url = getHtmlContentAsync client config httpError url
        member this.GetHtmlContent config url = getHtmlContentAsync client config httpError url |> Async.RunSynchronously
        member this.DownloadBinary config url outputFilePath = 
            initCommand url outputFilePath
            |> Util.Process.execute
            |> ignore
        member this.DownloadBinaryAsync config url outputFilePath = async {
            initCommand url outputFilePath
            |> Util.Process.execute
            |> ignore }
        member this.Error = httpError.Publish
    interface System.IDisposable with
        member this.Dispose() = 
            client.Dispose()

type ProxyWebClient (proxy: Util.API.Web.Http.Proxy, resources: Resources, temporaryDirPath: DirectoryPath) =
    let webDriver = resources.GetProxyWebDriver()
    let httpClient = initProxyHttpClient proxy
    let httpError = Event<API.Web.Http.ErrorDetails>()
    interface Util.API.Web.Client.IWebClient with
        member this.GetHtmlContentAsync config url = async { return Util.Web.Driver.loadContent webDriver url }
        member this.GetHtmlContent config url = Util.Web.Driver.loadContent webDriver url
        member this.DownloadBinary config url outputFilePath = 
            let config = config |> Util.Web.Driver.addCookies webDriver
            downloadBinaryAsync httpClient config temporaryDirPath httpError url outputFilePath  |> Async.RunSynchronously
        member this.DownloadBinaryAsync config url outputFilePath = 
            let config = config |> Util.Web.Driver.addCookies webDriver
            downloadBinaryAsync httpClient config temporaryDirPath httpError url outputFilePath
        member this.Error = httpError.Publish
    interface System.IDisposable with
        member this.Dispose() = 
            httpClient.Dispose()

let initWebClient 
    (resources: Resources) 
    (proxy: Util.API.Web.Http.Proxy)
    (temporaryDirPath: DirectoryPath)
    (clientType: Util.API.Web.Client.ClientType) =
    match clientType with
    | HttpClient -> new HttpClient (temporaryDirPath) :> Util.API.Web.Client.IWebClient
    | WebClient -> new WebClient(resources, temporaryDirPath) :> Util.API.Web.Client.IWebClient
    | ProxyHttpClient -> new ProxyHttpClient (proxy, temporaryDirPath) :> Util.API.Web.Client.IWebClient
    | ProxyWebClient -> new ProxyWebClient (proxy, resources, temporaryDirPath) :> Util.API.Web.Client.IWebClient
    | WebStream -> new WebStreamClient (temporaryDirPath) :> Util.API.Web.Client.IWebClient
