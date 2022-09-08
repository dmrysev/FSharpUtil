module Util.Test.Service.MessageQueueMonitor

open Util.Service.MessageQueueMonitor
open NUnit.Framework
open FsUnit

[<Test>]
let ``If message queue monitor service is running, adding a message to message queue, must trigger new message event``() =
    // ARRANGE
    let config = {
        Util.Service.MessageQueueMonitor.Config.QueueName = Util.Guid.generate()
        UpdateRate = System.TimeSpan.FromMilliseconds(1.0) }
    Util.MessageQueue.resetQueue config.QueueName
    let task, events = Util.Service.MessageQueueMonitor.init config
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
    Async.Start(task, taskCancellation.Token)

    // ACT
    Async.Start( async { do! Util.MessageQueue.enqueueAsync config.QueueName "test message" }, taskCancellation.Token)
    let message = events.NewMessage |> Async.AwaitEvent |> Async.RunSynchronously

    // ASSERT
    message.Content |> should equal "test message"

[<Test>]
let ``With message queue monitor initialized, adding a message to message queue, must trigger new message event``() =
    // ARRANGE
    let config = {
        Util.Service.MessageQueueMonitor.Config.QueueName = Util.Guid.generate()
        UpdateRate = System.TimeSpan.FromMilliseconds(1.0) }
    Util.MessageQueue.resetQueue config.QueueName
    use monitor = new Util.Service.MessageQueueMonitor.T (config)
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()

    // ACT
    Async.Start( async { do! Util.MessageQueue.enqueueAsync config.QueueName "test message" }, taskCancellation.Token)
    let message = monitor.NewMessage |> Async.AwaitEvent |> Async.RunSynchronously

    // ASSERT
    message.Content |> should equal "test message"

[<Test>]
let ``With message queue monitor tree initialized, adding a message to message queue, must trigger new message event``() =
    // ARRANGE
    let config: Util.Service.MessageQueueTreeMonitor.Config = {
        QueueName = Util.Guid.generate()
        UpdateRate = System.TimeSpan.FromMilliseconds(1.0)
        ResetQueue = false }
    use monitor = new Util.Service.MessageQueueTreeMonitor.T (config)
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
    let subQueueName = $"{config.QueueName}/sub_queue_1"
    Util.MessageQueue.ensureQueueInitialized subQueueName

    // ACT
    Async.Start( async { do! Util.MessageQueue.enqueueAsync subQueueName "test message" }, taskCancellation.Token)
    let message = monitor.NewMessage |> Async.AwaitEvent |> Async.RunSynchronously

    // ASSERT
    message.Content |> should equal "test message"
