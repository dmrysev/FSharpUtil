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

let findLinkskByPartialText (linkText: string) (html: HtmlDocument) =
    html
    |> findLinks
    |> Seq.filter (fun (text, _) -> text.Contains(linkText))
    |> Seq.map snd

let findLinksByRegex regexPattern html =
    let isMatch (link: string) =
        let regex = Regex regexPattern
        regex.IsMatch link
    findLinks html
    |> Seq.map snd
    |> Seq.filter isMatch

let descendants (nodeName: string) (node: HtmlNode) = node.Descendants nodeName

let descendant (nodeName: string) (node: HtmlNode)= 
    node.Descendants nodeName
    |> Seq.head

module HtmlDocument =
    let descendant (nodeName: string) (doc: HtmlDocument)= 
        doc.Descendants nodeName
        |> Seq.head
        