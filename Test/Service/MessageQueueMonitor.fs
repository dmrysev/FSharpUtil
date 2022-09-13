module Util.Test.Service.MessageQueueMonitor

open Util.Service.MessageQueueMonitor
open NUnit.Framework
open FsUnit

let spamQueueAsync queueName message = async {
    while true do
        do! Util.MessageQueue.enqueueAsync queueName message }

[<Test>]
let ``If message queue monitor service is running, adding a message to message queue, must trigger new message event``() =
    // ARRANGE
    let config: Util.Service.MessageQueueMonitor.Config = { QueueName = Util.Guid.generate() }
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
    use monitor = new Util.Service.MessageQueueMonitor.T (config)

    // ACT
    Async.Start (spamQueueAsync config.QueueName "test message", taskCancellation.Token)
    let message = monitor.NewMessage |> Async.AwaitEvent |> Async.RunSynchronously

    // ASSERT
    message.Content |> should equal "test message"

[<Test>]
let ``With message queue monitor initialized, adding a message to message queue, must trigger new message event``() =
    // ARRANGE
    let config: Util.Service.MessageQueueMonitor.Config = { QueueName = Util.Guid.generate() }
    use monitor = new Util.Service.MessageQueueMonitor.T (config)
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()

    // ACT
    Async.Start (spamQueueAsync config.QueueName "test message", taskCancellation.Token)
    let message = monitor.NewMessage |> Async.AwaitEvent |> Async.RunSynchronously

    // ASSERT
    message.Content |> should equal "test message"

[<Test>]
let ``With message queue monitor tree initialized, adding a message to message queue, must trigger new message event``() =
    // ARRANGE
    let config: Util.Service.MessageQueueTreeMonitor.Config = {
        QueueName = Util.Guid.generate()
        ResetQueue = false }
    use monitor = new Util.Service.MessageQueueTreeMonitor.T (config)
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
    let subQueueName = $"{config.QueueName}/sub_queue_1"
    Util.MessageQueue.ensureQueueInitialized subQueueName

    // ACT
    Async.Start (spamQueueAsync subQueueName "test message", taskCancellation.Token)
    let message = monitor.NewMessage |> Async.AwaitEvent |> Async.RunSynchronously

    // ASSERT
    message.Content |> should equal "test message"
