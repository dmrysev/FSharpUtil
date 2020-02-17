module Util.Http

open FSharp.Data
open System.IO

let downloadRawContent pageUrl =
    let cookies = 
        Http.Request(pageUrl).Cookies
        |> Map.toSeq
    Http.RequestString(pageUrl, httpMethod = "GET", cookies = cookies)

let getHtmlDocument url = 
    let content = downloadRawContent url
    HtmlDocument.Load url