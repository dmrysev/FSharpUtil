module Util.API.Web.Client

open Util.API.Web
open Util.Path

type ClientType = HttpClient | WebClient | ProxyHttpClient | ProxyWebClient | WebStream

type IWebClient =
    inherit System.IDisposable
    abstract member GetHtmlContent: Http.Config -> Url -> string
    abstract member GetHtmlContentAsync: Http.Config -> Url -> Async<string>
    abstract member DownloadBinary: Http.Config -> Url -> FilePath -> unit
    abstract member DownloadBinaryAsync: Http.Config -> Url -> FilePath -> Async<unit>
    abstract member Error: IEvent<Http.ErrorDetails>
