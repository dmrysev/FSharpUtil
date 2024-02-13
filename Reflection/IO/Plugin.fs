namespace Util.IO.Reflection

open Util
open Util.Path

type Plugin (pluginsDirPath: DirectoryPath) =
    interface API.Reflection.IPlugin with
        member this.Load<'a>() = Util.IO.Reflection.loadPlugins<'a> pluginsDirPath