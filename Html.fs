module Util.Html

open FSharp.Data
open System.Text.RegularExpressions

let findLinks (html: HtmlDocument) = 
    html.Descendants "a"
    |> Seq.choose (fun x -> 
        x.TryGetAttribute("href")
        |> Option.map (fun a -> x.InnerText(), a.Value())
    )

let findImages (html: HtmlDocument) = 
    html.Descendants "img"
    |> Seq.map (fun x -> 
        let alt = x.TryGetAttribute("alt") |> Option.map(fun a -> a.Value())
        let src = x.TryGetAttribute("src") |> Option.map(fun a -> a.Value())
        (alt.Value, src.Value)
    )

let findLinkByText html linkText =
    html
    |> findLinks
    |> Seq.find (fun (text, _) -> text = linkText)
    |> snd

let findLinksByRegex regexPattern html =
    let isMatch link =
        let regex = Regex regexPattern
        regex.IsMatch link

    findLinks html
    |> Seq.map snd
    |> Seq.filter isMatch