module Util.Test.MessageQueueRequest

open Util
open Util.IO.Path
open Util.MessageQueueCommand
open Util.MessageQueueQuery
open NUnit.Framework
open FsUnit

type UnionMessage = 
| FieldA of string
| FieldB of int

type ComplexMessage = {
    Path: FilePath }

[<Test>]
let ``Write request must be handled``() =
    // ARRANGE
    let config: MessageQueueCommand.Config = { 
        ListenerUpdareRate = System.TimeSpan.FromMilliseconds(1)
        ResetQueue = true }
    let request = Command<string>("util/test/request/write_request_test", config)
    let task, events = request.Subscribe()
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
    Async.Start(task, taskCancellation.Token)

    // ACT
    Async.Start(async { do! request.SendRequestAsync("test message") }, taskCancellation.Token)
    let message = events.NewRequest |> Async.AwaitEvent |> Async.RunSynchronously

    // ASSERT
    message |> should equal "test message"

[<Test>]
let ``Write request must be handled if message is union type``() =
    // ARRANGE
    let config: MessageQueueCommand.Config = { 
        ListenerUpdareRate = System.TimeSpan.FromMilliseconds(1)
        ResetQueue = true }
    let request = Command<UnionMessage>("util/test/request/write_request_union_type_test", config)
    let task, events = request.Subscribe()
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
    Async.Start(task, taskCancellation.Token)

    // ACT
    Async.Start(async { do! request.SendRequestAsync(UnionMessage.FieldA "test message") }, taskCancellation.Token)
    let message = events.NewRequest |> Async.AwaitEvent |> Async.RunSynchronously

    // ASSERT
    message |> should be (ofCase<@ FieldA @>)
    match message with
    | FieldA str -> str |> should equal "test message"
    | FieldB _ -> ()

[<Test>]
let ``Read request must be handled and recieve response``() =
    // ARRANGE
    let config: MessageQueueQuery.Config = { ListenerUpdateRate = System.TimeSpan.FromMilliseconds(1) }
    let request = Query<string, string>("util/test/request/read_request_test", config)
    let task, events = request.Subscribe()
    events.NewRequest.Add(fun requestHandler -> 
        let response = $"{requestHandler.RequestMessage}+test response"
        requestHandler.SendResponse(response))
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
    Async.Start(task, taskCancellation.Token)

    // ACT
    use response = request.SendRequest("test request")
    let result = response.WaitForOne()

    // ASSERT
    result |> should equal "test request+test response"

[<Test>]
let ``All read requests must be handled and recieve response``() =
    // ARRANGE
    let config: MessageQueueQuery.Config = { ListenerUpdateRate = System.TimeSpan.FromMilliseconds(1) }
    let request = Query<string, string>("util/test/request/read_request_test", config)
    let task, events = request.Subscribe()
    events.NewRequest.Add(fun requestHandler -> 
        let response = $"{requestHandler.RequestMessage}+test response"
        requestHandler.SendResponse(response))
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
    Async.Start(task, taskCancellation.Token)

    // ACT
    use response1 = request.SendRequest("test request 1")
    use response2 = request.SendRequest("test request 2")
    let result1 = response1.WaitForOne()
    let result2 = response2.WaitForOne()

    // ASSERT
    result1 |> should equal "test request 1+test response"
    result2 |> should equal "test request 2+test response"

[<Test>]
let ``Write request must support messages with complex types``() =
    // ARRANGE
    let config: MessageQueueCommand.Config = { 
        ListenerUpdareRate = System.TimeSpan.FromMilliseconds(1)
        ResetQueue = true }
    let request = Command<ComplexMessage>("util/test/request/write_request_test", config)
    let task, events = request.Subscribe()
    use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
    Async.Start(task, taskCancellation.Token)
    let testMessage: ComplexMessage = { Path = FilePath "/some/path/file.jpg" }

    // ACT
    Async.Start(async { do! request.SendRequestAsync testMessage }, taskCancellation.Token)
    let message = events.NewRequest |> Async.AwaitEvent |> Async.RunSynchronously

    // ASSERT
    message |> should equal testMessage
