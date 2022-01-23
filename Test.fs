module Util.Test

open Util.Http
open Util.IO.Path

let startLoopAsync func =
    let rec loop() = async {
        do! func()
        do! Async.Sleep 1
        do! loop() }
    loop() |> Async.Start

let startMessageQueueSpamming queueName message =
    let func() = Util.MessageQueue.enqueueAsync queueName message
    startLoopAsync func

let downloadBinaryFake (url: Url) (outputFilePath: FilePath) = Util.IO.File.create outputFilePath
    