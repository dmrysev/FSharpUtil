module Util.IO.MobileDevice

open Util.Path

let send (inputPath: Path) (outputDirPath: DirectoryPath) =
    Util.Process.run $"adb push --sync '{inputPath |> Util.Path.value}' '{outputDirPath.Value}'"
