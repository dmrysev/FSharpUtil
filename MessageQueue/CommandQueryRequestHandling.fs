module Util.CommandQueryRequestHandling

open Util
open Util.MessageQueueQuery
open Util.MessageQueueCommand

type QueryOptions = { 
    ResponseTimeout: System.TimeSpan
    MaxRetries: int }
with static member Default = {
        ResponseTimeout = System.TimeSpan.FromSeconds(30.0)
        MaxRetries = 0 }

let handleQueryResponse (options: QueryOptions) (query: Query<'RequestArgs, 'ResponseMessage>) (requester: MessageQueueQuery.Requester<'RequestArgs, 'ResponseMessage>) sendRequest =
    let responseTimeoutMilliseconds = options.ResponseTimeout.TotalMilliseconds |> int
    sendRequest()
    let rec retryFunc (attempt: int) = 
        try 
            Util.Log.debugInfo $"Sending new request for query {query.QueueName}"
            let task = Async.AwaitEvent requester.NewResponse
            let response = Async.RunSynchronously(task, responseTimeoutMilliseconds)
            match response with
            | MessageQueueQuery.Response.Success message -> MessageQueueQuery.Response.Success message
            | MessageQueueQuery.Response.Fail errorDetails -> 
                Util.Log.error errorDetails
                MessageQueueQuery.Response.Fail errorDetails
        with error ->
            Util.Log.error $"Response failed {query.QueueName}. {error.Message}"
            if attempt < options.MaxRetries then 
                Util.Log.error $"Retrying {query.QueueName}"
                sendRequest()
                retryFunc (attempt + 1)
            else 
                let errorDetails: MessageQueueQuery.Error = {
                    Code = 1
                    Responder = "None"
                    RequestName = query.QueueName
                    Message = error.Message }
                MessageQueueQuery.Response.Fail errorDetails 
    retryFunc(0)

let singleResponseQuery<'RequestArgs, 'ResponseMessage> (options: QueryOptions) (requestArgs: 'RequestArgs) (query: Query<'RequestArgs, 'ResponseMessage>): MessageQueueQuery.Response<'ResponseMessage> = 
    use requester = query.NewRequester()
    handleQueryResponse options query requester (fun _ -> requester.SendRequest requestArgs)

let singleResponseQueryNoArgs<'ResponseMessage> (options: QueryOptions) (query: Query<unit, 'ResponseMessage>): MessageQueueQuery.Response<'ResponseMessage> = 
    use requester = query.NewRequester()
    handleQueryResponse options query requester (fun _ -> requester.SendRequest())

type CommandOptions = { 
    ResponseTimeout: System.TimeSpan
    MaxRetries: int }
with static member Default = {
        ResponseTimeout = System.TimeSpan.FromSeconds(30.0)
        MaxRetries = 0 }

let sendCommand<'RequestArgs> (options: CommandOptions) (requestArgs: 'RequestArgs) (command: Command<'RequestArgs>): MessageQueueCommand.Response =
    let responseTimeoutMilliseconds = options.ResponseTimeout.TotalMilliseconds |> int
    use requester = command.NewRequester()
    requester.SendRequest requestArgs
    let rec retryFunc (attempt: int) = 
        try 
            Util.Log.debugInfo $"Sending new request for command {command.QueueName}"
            let task = Async.AwaitEvent requester.NewResponse
            let response = Async.RunSynchronously(task, responseTimeoutMilliseconds)
            match response with
            | MessageQueueCommand.Response.Success -> MessageQueueCommand.Response.Success
            | MessageQueueCommand.Response.Fail errorDetails -> 
                Util.Log.error errorDetails
                MessageQueueCommand.Response.Fail errorDetails
        with error ->
            Util.Log.error $"Response failed {command.QueueName}. {error.Message}"
            if attempt < options.MaxRetries then 
                Util.Log.error $"Retrying {command.QueueName}"
                requester.SendRequest requestArgs
                retryFunc (attempt + 1)
            else
                let errorDetails: Util.MessageQueueCommand.Error = {
                    Code = 1
                    Responder = "None"
                    RequestName = command.QueueName
                    Message = error.Message }
                MessageQueueCommand.Response.Fail errorDetails
    retryFunc(0)

let bindCommand<'RequestArgs> 
    (handleFunct: 'RequestArgs -> unit) 
    (command: MessageQueueCommand.Command<'RequestArgs>) =
    let currentExecutableFilePath = Util.IO.Reflection.currentExecutableFilePath()
    let responder = command.NewResponder()
    responder.NewRequest.Add (fun (requestHandler: MessageQueueCommand.RequestHandler<'RequestArgs>) ->
        Async.Start (async {
            try 
                Util.Log.debugInfo $"Handling new request for command {command.QueueName}"
                handleFunct requestHandler.RequestArgs
                requestHandler.SendSuccessResponse()
            with error ->
                let errorDetails: MessageQueueCommand.Error = {
                    Code = 1
                    RequestName = command.QueueName
                    Responder = currentExecutableFilePath.Value
                    Message = error.Message }
                Util.Log.error $"Request failed for command {command.QueueName}"
                Util.Log.error errorDetails
                requestHandler.SendFailResponse errorDetails }) )
    responder :> System.IDisposable

let bindQuery<'RequestArgs, 'ResponseMessage>
    (handleFunct: 'RequestArgs -> 'ResponseMessage) 
    (query: MessageQueueQuery.Query<'RequestArgs, 'ResponseMessage>) =
    let currentExecutableFilePath = Util.IO.Reflection.currentExecutableFilePath()
    let responder = query.NewResponder()
    responder.NewRequest.Add (fun (requestHandler: MessageQueueQuery.RequestHandler<'RequestArgs, 'ResponseMessage>) ->
        Async.Start (async {
            try  
                Util.Log.debugInfo $"Handling new request for query {query.QueueName}"
                handleFunct requestHandler.RequestArgs |> requestHandler.SendResponse
            with error ->
                let errorDetails: MessageQueueQuery.Error = {
                    Code = 1
                    RequestName = query.QueueName
                    Responder = currentExecutableFilePath.Value
                    Message = error.Message }
                Util.Log.error $"Request failed for query {query.QueueName}"
                Util.Log.error errorDetails
                requestHandler.SendFailResponse errorDetails }) )
    responder :> System.IDisposable
