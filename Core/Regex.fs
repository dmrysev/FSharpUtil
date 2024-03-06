module Util.Regex

open System.Text.RegularExpressions

let matchValue (pattern: string) (str: string) = 
    (Regex pattern).Match str 
    |> fun x -> x.Value

let matchFloat (str: string) = matchValue @"\d+\.\d+" str

let isMatch (pattern: string) (str: string) =
    Regex.IsMatch(str, pattern)

let isMatchIgnoreCase (pattern: string) (str: string) =
    let regex = Regex(pattern, RegexOptions.IgnoreCase)
    regex.IsMatch(str)
