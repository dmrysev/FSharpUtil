module Util.IO.Reflection

open Util.Path
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
