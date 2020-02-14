module Util.Html

open FSharp.Data

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

let findLinkByText html textValue =
    html
    |> findLinks
    |> Seq.find (fun (text, _) -> text = textValue)
    |> snd