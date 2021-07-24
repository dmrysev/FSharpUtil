module Util.Environment

open Util.IO.Path
open System.Runtime.InteropServices

module SpecialFolder =
    let home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)
    let applicationData = home/".local/share"
    let temporary = "/tmp"

module OS =
    let isLinux() =
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
