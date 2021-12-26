module Util.String
open System.Text.RegularExpressions

let extractInt text = Regex.Match(text, @"\d+").Value |> int
let replace (oldValue: string) (newValue: string) (text: string) = text.Replace(oldValue, newValue)
let split (separator: string) (text: string) = text.Split([|separator|], System.StringSplitOptions.None)
let toLower (str: string) = str.ToLower()
let removeLastCharacter (count: int) (str: string) = str.Remove(str.Length - count)
let removeFiratCharacter (str: string) = str.Remove(0, 1)
let contains (subString: string) (str: string) = str.Contains(subString)
let startsWith (subString: string) (str: string) = str.StartsWith(subString)
let endsWith (subString: string) (str: string) = str.EndsWith(subString)

let strip chars str =
    Seq.fold(fun (str: string) chr ->
        str.Replace(chr |> System.Char.ToUpper |> string, "").Replace(chr |> System.Char.ToLower |> string, ""))
        str chars

let remove (toRemove: string) (text: string) =
    if toRemove = "" then text
    else text.Replace(toRemove, "")