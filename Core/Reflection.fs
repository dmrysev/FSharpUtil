module Util.Reflection

open FSharp.Reflection

module Union =
    let toString (x:'a) = 
        match FSharpValue.GetUnionFields(x, typeof<'a>) with
        | case, _ -> case.Name
    let fromString<'a> (s:string) =
        match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
        |[|case|] -> FSharpValue.MakeUnion(case,[||]) :?> 'a
        |_ -> raise (System.ArgumentException($"{s}"))    

    let tryFromString<'a> (s:string) =
        match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
        |[|case|] -> Some(FSharpValue.MakeUnion(case,[||]) :?> 'a)
        |_ -> None

    let casesStrings<'a>() =
        typeof<'a>
        |> FSharp.Reflection.FSharpType.GetUnionCases
        |> Seq.map (fun x -> x.Name) 
