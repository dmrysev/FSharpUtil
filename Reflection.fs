module Util.Reflection

open Util.IO.Path
open System
open System.Reflection

let loadPlugins<'a>(pluginsDirPath: DirectoryPath) =
    Util.IO.Directory.listDirectories pluginsDirPath
    |> Seq.map (fun pluginDirPath -> 
        Util.IO.Directory.listFiles pluginDirPath
        |> Seq.map FilePath.value
        |> Seq.filter (Util.Regex.isMatch @".+\.dll") )
    |> Seq.concat
    |> Seq.map (fun path -> (Assembly.LoadFile path).GetTypes() )
    |> Seq.concat
    |> Seq.filter(fun assemblyType -> typeof<'a>.IsAssignableFrom(assemblyType) )
    |> Seq.map(fun assemblyType -> Activator.CreateInstance(assemblyType) )
    |> Seq.filter(fun x -> not (x |> isNull) )
    |> Seq.cast<'a>
