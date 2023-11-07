module Util.Test

open Util.Path

let startLoopAsync func =
    let rec loop() = async {
        do! func()
        do! Async.Sleep 1
        do! loop() }
    loop() |> Async.Start

let downloadBinaryFake (url: Url) (outputFilePath: FilePath) = Util.IO.File.create outputFilePath

type EventMonitor<'a>(event: IEvent<'a>) as this =
    do event.Add(fun (value: 'a) -> 
        this.TriggerCount <- this.TriggerCount + 1
        this.LastValue <- Some value)
    member val TriggerCount = 0 with get, set
    member val LastValue: 'a option = None with get, set
    