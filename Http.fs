module Util.Http

open Util.IO.Path
open FSharp.Data
open System.IO

type Url (url: string) =
    let url = 
        if url = "" then raise (System.ArgumentException("Url can't be empty"))
        url |> Url.fix
    member this.Value = url
    member this.DomainName = System.Uri(this.Value).Host
    member this.IsMath (regexPattern: string) = this.Value |> Util.Regex.isMatch regexPattern
    member this.IsDomainMatch (otherUrl: Url) = this.DomainName = otherUrl.DomainName
    member this.FileName = this.Value |> System.IO.Path.GetFileName |> FileName

    static member value (url: Url) = url.Value
    static member fix (url: string) = url |> Util.String.removeLastCharacterIfEquals "/"

    static member (/) (urlPath1: Url, urlPath2: Url) = 
        let combined = sprintf "%s/%s" urlPath1.Value urlPath2.Value
        Url combined

    static member (/) (urlPath1: Url, urlPath2: string) = 
        let combined = sprintf "%s/%s" urlPath1.Value urlPath2
        Url combined

    static member (/) (urlPath1: Url, urlPath2: int) = 
        let combined = sprintf "%s/%i" urlPath1.Value urlPath2
        Url combined

    static member (+) (urlPath: Url, value: string) = urlPath.Value + value |> Url
    static member (+) (urlPath: Url, value: int) = sprintf "%s%i" urlPath.Value value |> Url

    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? Url as u -> this.Value = u.Value
        | _ -> false

module Url =
    let isMatch (regexPattern: string) (url: Url) = url.IsMath regexPattern
    let extenstion (url: Url) = System.IO.Path.GetExtension url.Value
    let domainName (url: Url) = url.DomainName
    let fileName (url: Url) = url.FileName
    let remove (toRemove: string) (url: Url) =
        url.Value
        |> Util.String.remove toRemove
        |> Url
