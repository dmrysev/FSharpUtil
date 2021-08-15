module Util.Test.MessageQueue

open Util.MessageQueue
open NUnit.Framework
open FsUnit

let queue1 = "queue_1"
let queue2 = "queue_1"

[<SetUp>]
let setUp() =
    resetQueue queue1
    resetQueue queue2

[<Test>]
let ``MessageQueue must satisfy FIFO property``() =
    enqueue queue1 "message 1"
    enqueue queue1 "message 2"
    enqueue queue1 "message 3"
    dequeue queue1 |> should equal "message 1"
    dequeue queue1 |> should equal "message 2"
    dequeue queue1 |> should equal "message 3"

[<Test>]
let ``MessageQueue must support multiple queues``() =
    enqueue queue1 "message 1"
    enqueue queue2 "message 2"
    dequeue queue1 |> should equal "message 1"
    dequeue queue2 |> should equal "message 2"

[<Test>]
let ``MessageQueue must support async operations for same queue``() =
    [enqueueAsync queue1 "message 1"; enqueueAsync queue1 "message 2"]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore

    let result =
        dequeueAsync queue1
        |> Seq.replicate 2
        |> Async.Parallel
        |> Async.RunSynchronously

    result |> should equivalent ["message 1"; "message 2"]

[<Test>]
let ``MessageQueue must support sub directory queue names``() =
    resetQueue "A/B/queue_1"
    enqueue "A/B/queue_1" "message 1"
    dequeue "A/B/queue_1" |> should equal "message 1"
