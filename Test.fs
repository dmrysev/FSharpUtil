module Util.Test

let startLoopAsync func =
    let rec loop() = async {
        do! func()
        do! Async.Sleep 1
        do! loop() }
    loop() |> Async.Start

let startMessageQueueSpamming queueName message =
    let func() = Util.MessageQueue.enqueueAsync queueName message
    startLoopAsync func
    