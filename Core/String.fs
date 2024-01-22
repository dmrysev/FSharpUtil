module Util.String

open System.Text.RegularExpressions

let extractInt text = Regex.Match(text, @"\d+").Value |> int
let replace (oldValue: string) (newValue: string) (text: string) = text.Replace(oldValue, newValue)
let split (separator: string) (text: string) = text.Split([|separator|], System.StringSplitOptions.None)
let toUpper (str: string) = str.ToUpper()
let toLower (str: string) = str.ToLower()
let removeLastCharacters (count: int) (str: string) = str.Remove(str.Length - count)
let removeLastCharacter (str: string) = str.Remove(str.Length - 1)
let removeFirstCharacter (str: string) = str.Remove(0, 1)
let contains (subString: string) (str: string) = str.Contains(subString)
let startsWith (subString: string) (str: string) = str.StartsWith(subString)
let endsWith (subString: string) (str: string) = str.EndsWith(subString)
let defaultIfEmpty (subString: string) (str: string) = if str = "" then subString else str
let head (endIndex: int) (str: string) = str.Substring (0, endIndex)
let headLimit (maxCount: int) (str: string) = 
    if str.Length > maxCount then str.Substring (0, maxCount - 1)
    else str
let tail (startIndex: int) (str: string) = str.Substring (startIndex, str.Length - startIndex)
let slice (startIndex: int) (endIndex: int) (str: string) = str.Substring (startIndex, endIndex - startIndex + 1)

let removeFirstCharacterIfEquals (subString: string) (str: string) =
    if str |> startsWith subString then str |> removeFirstCharacter
    else str

let removeLastCharacterIfEquals (subString: string) (str: string) =
    if str |> endsWith subString then str |> removeLastCharacter
    else str

let strip chars str =
    Seq.fold(fun (str: string) chr ->
        str.Replace(chr |> System.Char.ToUpper |> string, "").Replace(chr |> System.Char.ToLower |> string, ""))
        str chars

let remove (toRemove: string) (text: string) =
    if toRemove = "" then text
    else text.Replace(toRemove, "")

let insert startIndex value (str: string) = str.Insert(startIndex, value) 
