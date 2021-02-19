module Util.Environment

open Util.IO.Path

module SpecialFolder =
    let homeDirPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)
    let applicationDataDirPath = homeDirPath/".local/share/LinuxUtil"
