module Util.Regex

open System.Text.RegularExpressions

let MatchValue (pattern: string) (str: string) = 
    (Regex pattern).Match str 
    |> fun x -> x.Value
