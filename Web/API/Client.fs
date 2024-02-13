module Util.API.Web.Client

open Util.API.Web
open Util.Path

type ClientType = HttpClient | WebClient | ProxyHttpClient | ProxyWebClient | WebStream

type IWebClient =
    inherit System.IDisposable
    abstract GetHtmlContent: Http.Config -> Url -> string
    abstract GetHtmlContentAsync: Http.Config -> Url -> Async<string>
    abstract DownloadBinary: Http.Config -> Url -> FilePath -> unit
    abstract DownloadBinaryAsync: Http.Config -> Url -> FilePath -> Async<unit>
    abstract Error: IEvent<Http.ErrorDetails>
