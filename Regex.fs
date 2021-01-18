module Util.Regex

open System.Text.RegularExpressions

let matchValue (pattern: string) (str: string) = 
    (Regex pattern).Match str 
    |> fun x -> x.Value

let isMatch (pattern: string) (str: string) =
    Regex.IsMatch(str, pattern)
