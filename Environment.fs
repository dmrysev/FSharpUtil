module Util.Environment

open Util.IO.Path
open System.Runtime.InteropServices

module SpecialFolder =
    let home = DirectoryPath (System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile))
    let applicationData = home/DirectoryPath ".local/share"
    let temporary = DirectoryPath "/tmp"

module OS =
    let isLinux() =
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
