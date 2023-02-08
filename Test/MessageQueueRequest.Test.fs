module Util.Test.MessageQueueRequest

open Util
open Util.IO.Path
open Util.MessageQueueCommand
open Util.MessageQueueQuery
open NUnit.Framework
open FsUnit

type UnionMessage = FieldA of string | FieldB of int
type ComplexMessage = { Path: FilePath }

let initQueueName() = $"util/test/{Util.Guid.generate()}"

[<Test>]
let ``Command request must be handled``() =
    // ARRANGE
    let config: MessageQueueCommand.Config = { 
        ResponseTimeout = Some (System.TimeSpan.FromMilliseconds 500)
        ResponseMaxRetries = None }
    let command = Command<string>(initQueueName(), config)

    // ACT
    let mutable requestArgs = ""
    use responder = command.NewResponder()
    responder.NewRequest.Add(fun requestHandler -> 
        requestArgs <- requestHandler.RequestArgs
        requestHandler.SendSuccessResponse() )
    let response = command.SendRequest "test request"

    // ASSERT
    match response with
    | MessageQueueCommand.Response.Success -> ()
    | _ -> failwith "Invalid case"
    requestArgs |> should equal "test request"

[<Test>]
let ``Query request must be handled``() =
    // ARRANGE
    let config: MessageQueueQuery.Config = { 
        ResponseTimeout = Some (System.TimeSpan.FromMilliseconds 100)
        ResponseMaxRetries = None }
    let query = MessageQueueQuery.Query<string, string>(initQueueName(), config)

    // ACT
    use responder = query.NewResponder()
    responder.NewRequest.Add(fun requestHandler -> 
        $"{requestHandler.RequestArgs}+test response" |> requestHandler.SendResponse )
    let response = query.SingleResponseRequest "test request" 

    // ASSERT
    match response with
    | MessageQueueQuery.Response.Success message -> message |> should equal "test request+test response"
    | _ -> failwith "Invalid case"

[<Test>]
let ``Multiple query requests must be handled and recieve response``() =
    // ARRANGE
    let config: MessageQueueQuery.Config = { 
        ResponseTimeout = Some (System.TimeSpan.FromMilliseconds 100)
        ResponseMaxRetries = None }
    let query = MessageQueueQuery.Query<string, string>(initQueueName(), config)

    // ACT
    use responder = query.NewResponder()
    responder.NewRequest.Add(fun requestHandler -> 
        $"{requestHandler.RequestArgs}+test response" |> requestHandler.SendResponse )
    let response1 = query.SingleResponseRequest "test request 1"
    let response2 = query.SingleResponseRequest "test request 2"

    // ASSERT
    match response1 with
    | MessageQueueQuery.Response.Success message -> message |> should equal "test request 1+test response"
    | _ -> failwith "Invalid case"
    match response2 with
    | MessageQueueQuery.Response.Success message -> message |> should equal "test request 2+test response"
    | _ -> failwith "Invalid case"

[<Test>]
let ``Query request must be support case with no request arguments``() =
    // ARRANGE
    let config: MessageQueueQuery.Config = { 
        ResponseTimeout = Some (System.TimeSpan.FromMilliseconds 100)
        ResponseMaxRetries = None }
    let query = MessageQueueQuery.Query<unit, string>(initQueueName(), config)

    // ACT
    use responder = query.NewResponder()
    responder.NewRequest.Add(fun requestHandler -> requestHandler.SendResponse "test response" )
    let response = query.SingleResponseRequest()

    // ASSERT
    match response with
    | MessageQueueQuery.Response.Success message -> message |> should equal "test response"
    | _ -> failwith "Invalid case"
