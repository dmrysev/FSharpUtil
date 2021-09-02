module Util.String
open System.Text.RegularExpressions

let strip chars str =
    Seq.fold(fun (str: string) chr ->
        str.Replace(chr |> System.Char.ToUpper |> string, "").Replace(chr |> System.Char.ToLower |> string, ""))
        str chars

let extractInt text = Regex.Match(text, @"\d+").Value |> int

let replace (oldValue: string) (newValue: string) (text: string) = text.Replace(oldValue, newValue)

let split (separator: string) (text: string) = 
    text.Split([|separator|], System.StringSplitOptions.None)

let toLower (str: string) = str.ToLower()

let removeLastCharacter (str: string) (count: int) =
    str.Remove(str.Length - count)

let contains (subString: string) (str: string) =
    str.Contains(subString)

let startsWith (subString: string) (str: string) =
    str.StartsWith(subString)
