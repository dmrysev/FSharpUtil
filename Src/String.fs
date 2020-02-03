module Util.String

let stripChars chars str =
    Seq.fold(fun (str: string) chr ->
        str.Replace(chr |> System.Char.ToUpper |> string, "").Replace(chr |> System.Char.ToLower |> string, ""))
        str chars