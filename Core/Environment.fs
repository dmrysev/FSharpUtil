module Util.Environment

open Util.Path
open System

module SpecialFolder =
    let home = DirectoryPath (System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile))
    let applicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) |> DirectoryPath
    let temporary = System.IO.Path.GetTempPath() |> DirectoryPath
    let currentAssembly =
        System.Reflection.Assembly.GetExecutingAssembly().Location |> FilePath
        |> FilePath.directoryPath
