module Util.String
open System.Text.RegularExpressions

let strip chars str =
    Seq.fold(fun (str: string) chr ->
        str.Replace(chr |> System.Char.ToUpper |> string, "").Replace(chr |> System.Char.ToLower |> string, ""))
        str chars

let extractInt text = Regex.Match(text, @"\d+").Value |> int