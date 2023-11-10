module Util.Web.Driver

open Util.Path

type Config = {
    Proxy: Util.API.Web.Http.Proxy option
    WebDriverLocation: DirectoryPath }
with static member Default = {
        Proxy = None
        WebDriverLocation = DirectoryPath "/usr/bin" }

let initProxySettings (proxyConf: Util.API.Web.Http.Proxy) =
    let proxy = OpenQA.Selenium.Proxy()
    proxy.SocksProxy <- $"{proxyConf.Ip}:{proxyConf.Port}"
    proxy.SocksVersion <- 5
    proxy
    
let initChrome(config: Config) =
    let options = OpenQA.Selenium.Chrome.ChromeOptions()
    match config.Proxy with
    | Some proxy -> options.Proxy <- initProxySettings proxy
    | None -> ()
    new OpenQA.Selenium.Chrome.ChromeDriver(options) :> OpenQA.Selenium.IWebDriver

let initChromeRemote(config: Config) (port: int) =
    let options = OpenQA.Selenium.Chrome.ChromeOptions()
    options.DebuggerAddress <- $"127.0.0.1:{port}"
    match config.Proxy with
    | Some proxy -> options.Proxy <- initProxySettings proxy
    | None -> ()
    new OpenQA.Selenium.Chrome.ChromeDriver(config.WebDriverLocation.Value, options) :> OpenQA.Selenium.IWebDriver

let initFirefox(config: Config) =
    let options = OpenQA.Selenium.Firefox.FirefoxOptions()
    match config.Proxy with
    | Some proxy -> options.Proxy <- initProxySettings proxy
    | None -> ()
    new OpenQA.Selenium.Firefox.FirefoxDriver(options) :> OpenQA.Selenium.IWebDriver

let loadContent (webDriver: OpenQA.Selenium.IWebDriver) (url: Url) =
    webDriver.Navigate().GoToUrl(url.Value)
    webDriver.PageSource

let addCookies (webDriver: OpenQA.Selenium.IWebDriver) (config: Util.API.Web.Http.Config) =
    let cookieString =
        webDriver.Manage().Cookies.AllCookies
        |> Seq.map (fun c -> $"{c.Name}={c.Value}")
        |> String.concat ";"
    let cookiesHeader = ("Cookie", cookieString)
    let headers =
        match config.Headers with
        | Some headers -> headers |> Util.Seq.appendItem cookiesHeader
        | None -> [ cookiesHeader ]
    { config with Headers = Some headers }
