module Util.Reflection

open Util.IO.Path
open System
open System.Reflection

let loadAssembly assemblyFilePath =
    try 
        let types = (Assembly.LoadFile assemblyFilePath).GetTypes()
        Some types
    with | :? System.BadImageFormatException -> None

let loadPlugins<'a>(pluginsDirPath: DirectoryPath) =
    if pluginsDirPath |> Util.IO.Directory.exists then
        Util.IO.Directory.listDirectories pluginsDirPath
        |> Seq.collect (fun pluginDirPath -> 
            Util.IO.Directory.listFiles pluginDirPath
            |> Seq.map FilePath.value
            |> Seq.filter (Util.Regex.isMatch @".+\.dll") )
        |> Seq.choose loadAssembly
        |> Seq.concat
        |> Seq.filter(fun assemblyType -> typeof<'a>.IsAssignableFrom(assemblyType) )
        |> Seq.map(fun assemblyType -> Activator.CreateInstance(assemblyType) )
        |> Seq.filter(fun x -> not (x |> isNull) )
        |> Seq.cast<'a>
    else Seq.empty

let currentExecutableFilePath() = System.Reflection.Assembly.GetExecutingAssembly().Location |> FilePath
