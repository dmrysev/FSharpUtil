module Util.IO.Environment

open Util.Path
open System
open System.Runtime.InteropServices

module OS =
    let isLinux() =
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux)

module BatteryInfo =
    type BatteryInfo = {
        Id: int
        State: State
        Charge: int }
    and State = Charging | Discharging | Unknown

    let getBatteryInfo() =
        let parseBatteryInfo acpiOutput =
            if acpiOutput = "" then { BatteryInfo.Id = -1; State = State.Unknown; Charge = 0 }
            else
                let id =
                    acpiOutput 
                    |> Util.String.split ":" 
                    |> Seq.head
                    |> Util.String.extractInt
                let infoChunks = 
                    acpiOutput
                    |> Util.String.split ","
                let state =
                    let stateStr = infoChunks[0] |> Util.String.toLower
                    if stateStr |> Util.String.contains "charging" then State.Charging
                    elif stateStr |> Util.String.contains "discharging" then State.Discharging
                    else State.Unknown
                let charge = infoChunks[1] |> Util.String.extractInt
                { BatteryInfo.Id = id; State = state; Charge = charge }

        Util.Process.execute "acpi"
        |> Util.String.split "\n"
        |> Seq.map parseBatteryInfo

module XServer =
    let isRunning() = 
        let script = """
            if xset q &>/dev/null; then echo 1
            else echo 0
            fi
        """
        let result = Util.Process.execute script
        if result = "1" then true
        else false

module WindowManagement =
    let windowWithTitleExists title =
        Util.Process.execute "wmctrl -l"
        |> Util.String.split "\n"
        |> Seq.exists (Util.String.contains title)

    let setNoTaskbar title = 
        Util.Process.run $"wmctrl -r '{title}' -b add,skip_taskbar"
