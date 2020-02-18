module Util.Http

open FSharp.Data
open System.IO

let downloadRawContent (pageUrl: string) (additionalCookies: seq<string*string>) =
    let allCookies = 
        Http.Request(pageUrl).Cookies 
        |> Map.toSeq
        |> Seq.append additionalCookies
    Http.RequestString(pageUrl, httpMethod = "GET", cookies = allCookies)

let getHtmlDocument (url: string) = 
    HtmlDocument.Load url