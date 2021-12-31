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

module BatteryInfo =
    type BatteryInfo = {
        Id: int
        State: State
        Charge: int }
    and State = Charging | Discharging | Unknown

    let getBatteryInfo() =
        let parseBatteryInfo acpiOutput =
            let id =
                acpiOutput 
                |> Util.String.split ":" 
                |> Seq.head
                |> Util.String.extractInt
            let infoChunks = 
                acpiOutput
                |> Util.String.split ","
            let state =
                let stateStr = infoChunks.[0] |> Util.String.toLower
                if stateStr |> Util.String.contains "charging" then State.Charging
                elif stateStr |> Util.String.contains "discharging" then State.Discharging
                else State.Unknown
            let charge = infoChunks.[1] |> Util.String.extractInt
            { BatteryInfo.Id = id; State = state; Charge = charge }

        Util.Process.execute "acpi"
        |> Util.String.split "\n"
        |> Seq.map parseBatteryInfo

