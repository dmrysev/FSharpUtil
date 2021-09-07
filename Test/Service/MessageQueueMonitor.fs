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
    let task, events = Util.Service.MessageQueueMonitor.init config
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
    Async.Start(task, taskCancellation.Token)

    // ACT
    Util.MessageQueue.enqueue config.QueueName "test message"
    let message = events.NewMessage |> Async.AwaitEvent |> Async.RunSynchronously

    // ASSERT
    message.Content |> should equal "test message"
