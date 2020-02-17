module Util.Http

open FSharp.Data
open System.IO

let downloadRawContent (pageUrl: string) (additionalCookies: seq<string*string> option) =
    let userCookies = 
        match additionalCookies with
        | Some c -> c
        | None -> Seq.empty
    let allCookies = 
        Http.Request(pageUrl).Cookies 
        |> Map.toSeq
        |> Seq.append userCookies
    Http.RequestString(pageUrl, httpMethod = "GET", cookies = allCookies)

let getHtmlDocument (url: string) = 
    HtmlDocument.Load url