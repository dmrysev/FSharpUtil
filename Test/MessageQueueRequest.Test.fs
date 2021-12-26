module Util.Test.MessageQueueRequest

open Util.MessageQueueRequest
open NUnit.Framework
open FsUnit

[<Test>]
let ``Write request``() =
    // ARRANGE
    let config = { 
        WriteRequestConfig.ListenerUpdareRate = System.TimeSpan.FromMilliseconds(1)
        ResetQueue = true }
    let request = WriteRequest<string>("util/test/request/write_request_test", config)
    let task, events = request.Subscribe()
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
    Async.Start(task, taskCancellation.Token)

    // ACT
    Util.Test.startLoopAsync (fun _ -> async { 
        do! request.SendRequestAsync("test message")
        do! Async.Sleep 10 })
    let message = events.NewRequest |> Async.AwaitEvent |> Async.RunSynchronously

    // ASSERT
    message |> should equal "test message"

[<Test>]
let ``Read request``() =
    // ARRANGE
    let config = { ReadRequestConfig.ListenerUpdareRate = System.TimeSpan.FromMilliseconds(1) }
    let request = ReadRequest<string, string>("util/test/request/write_request_test", config)
    let task, events = request.Subscribe()
    events.NewRequest.Add(fun message -> request.SendResponse("test response"))
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
    Async.Start(task, taskCancellation.Token)

    // ACT
    let response = request.SendRequest("test request")
    let result = response.WaitForOne()

    // ASSERT
    result |> should equal "test response"
