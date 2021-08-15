module Util.Test.MessageQueue

open Util.MessageQueue
open NUnit.Framework
open FsUnit

let queue1 = "queue_1"

[<SetUp>]
let setUp() =
    resetQueue queue1

[<Test>]
let ``MessageQueue must satisfy all FIFO property``() =
    enqueue queue1 "message 1"
    enqueue queue1 "message 2"
    enqueue queue1 "message 3"
    dequeue queue1 |> should equal "message 1"
    dequeue queue1 |> should equal "message 2"
    dequeue queue1 |> should equal "message 3"
