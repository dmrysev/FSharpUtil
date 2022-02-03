module Util.Test.MessageQueue

open Util.MessageQueue
open NUnit.Framework
open FsUnit

let queue1 = "queue_1"
let queue2 = "queue_2"

[<SetUp>]
let setUp() =
    resetQueue queue1
    resetQueue queue2

[<Test>]
let ``Enqueue a message and then dequeue it back, must return the same message``() =
    enqueue queue1 "message 1"
    dequeue queue1 |> Option.isSome |> should be True

[<Test>]
let ``If no message was equeued, trying to dequeue, must return None``() =
    dequeue queue1 |> Option.isNone |> should be True

[<Test>]
let ``Message queue must satisfy FIFO property``() =
    enqueue queue1 "message 1"
    enqueue queue1 "message 2"
    enqueue queue1 "message 3"
    dequeue queue1 |> Util.Option.ofSome |> should equal "message 1"
    dequeue queue1 |> Util.Option.ofSome |> should equal "message 2"
    dequeue queue1 |> Util.Option.ofSome |> should equal "message 3"

[<Test>]
let ``Message queue must support multiple queues``() =
    enqueue queue1 "message 1"
    enqueue queue2 "message 2"
    dequeue queue1 |> Util.Option.ofSome |> should equal "message 1"
    dequeue queue2 |> Util.Option.ofSome |> should equal "message 2"

[<Test>]
let ``Message queue must support async operations for same queue``() =
    [enqueueAsync queue1 "message 1"; enqueueAsync queue1 "message 2"]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore

    let result =
        dequeueAsync queue1
        |> Seq.replicate 2
        |> Async.Parallel
        |> Async.RunSynchronously

    result |> Seq.map Util.Option.ofSome |> should equivalent ["message 1"; "message 2"]

[<Test>]
let ``Message queue must support sub directory queue names``() =
    resetQueue "A/B/queue_1"
    enqueue "A/B/queue_1" "message 1"
    dequeue "A/B/queue_1" |> Util.Option.ofSome |> should equal "message 1"

[<Test>]
let ``Message queue count messages``() =
    // ARRANGE
    enqueue queue1 "message 1"
    enqueue queue1 "message 2"
    enqueue queue1 "message 3"

    // ACT & ASSERT
    countMessages queue1 |> should equal 3
    countMessages queue2 |> should equal 0
