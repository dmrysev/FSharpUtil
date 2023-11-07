module Util.Reflection

open Util.Path
open FSharp.Reflection
open System
open System.Reflection

let loadPlugins<'a>(pluginsDirPath: DirectoryPath) =
    if pluginsDirPath |> Util.IO.Directory.exists then
        Util.IO.Directory.listFilesRecursivePattern pluginsDirPath "*.dll"
        |> Seq.choose (fun assemblyFilePath ->
            try Some ((Assembly.LoadFile assemblyFilePath.Value).GetTypes())
            with error -> None)
        |> Seq.concat
        |> Seq.filter(fun assemblyType -> typeof<'a>.IsAssignableFrom(assemblyType) )
        |> Seq.map(fun assemblyType -> Activator.CreateInstance(assemblyType) )
        |> Seq.filter(fun x -> not (x |> isNull) )
        |> Seq.cast<'a>
    else Seq.empty

let currentExecutableFilePath() = System.Reflection.Assembly.GetExecutingAssembly().Location |> FilePath

module Union =
    let toString (x:'a) = 
        match FSharpValue.GetUnionFields(x, typeof<'a>) with
        | case, _ -> case.Name
    let fromString<'a> (s:string) =
        match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
        |[|case|] -> FSharpValue.MakeUnion(case,[||]) :?> 'a
        |_ -> raise (ArgumentException($"{s}"))    

    let tryFromString<'a> (s:string) =
        match FSharpType.GetUnionCases typeof<'a> |> Array.filter (fun case -> case.Name = s) with
        |[|case|] -> Some(FSharpValue.MakeUnion(case,[||]) :?> 'a)
        |_ -> None

    let casesStrings<'a>() =
        typeof<'a>
        |> FSharp.Reflection.FSharpType.GetUnionCases
        |> Seq.map (fun x -> x.Name) 
