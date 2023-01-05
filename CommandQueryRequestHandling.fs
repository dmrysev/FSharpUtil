module Util.CommandQueryRequestHandling

open Util
open Util.MessageQueueQuery
open Util.MessageQueueCommand

let responseTimeoutMilliseconds = System.TimeSpan.FromSeconds(30).TotalMilliseconds |> int
let responseMaxRetries = 0

let singleResponseQuery<'RequestArgs, 'ResponseMessage> (requestArgs: 'RequestArgs) (query: Query<'RequestArgs, 'ResponseMessage>): 'ResponseMessage option = 
    use requester = query.NewRequester()
    requester.SendRequest requestArgs
    let rec retryFunc (attempt: int) = 
        try 
            Util.Log.debugInfo $"Sending new request for query {query.QueueName}"
            let task = Async.AwaitEvent requester.NewResponse
            let response = Async.RunSynchronously(task, responseTimeoutMilliseconds)
            match response with
            | MessageQueueQuery.Response.Success message -> Some message
            | MessageQueueQuery.Response.Fail errorDetails -> 
                Util.Log.error errorDetails
                None
        with error ->
            Util.Log.error $"Response failed {query.QueueName}. {error.Message}"
            if attempt < responseMaxRetries then 
                Util.Log.error $"Retrying {query.QueueName}"
                requester.SendRequest requestArgs
                retryFunc (attempt + 1)
            else None 
    retryFunc(0)

let singleResponseQueryNoArgs<'ResponseMessage> (query: Query<unit, 'ResponseMessage>): 'ResponseMessage option = 
    use requester = query.NewRequester()
    requester.SendRequest()
    let rec retryFunc (attempt: int) =
        try 
            Util.Log.debugInfo $"Sending new request for query {query.QueueName}"
            let task = Async.AwaitEvent requester.NewResponse
            let response = Async.RunSynchronously(task, responseTimeoutMilliseconds)
            match response with
            | MessageQueueQuery.Response.Success message -> Some message
            | MessageQueueQuery.Response.Fail errorDetails -> 
                Util.Log.error errorDetails
                None
        with error ->
            Util.Log.error $"Response failed {query.QueueName}. {error.Message}"
            if attempt < responseMaxRetries then 
                Util.Log.error $"Retrying {query.QueueName}"
                requester.SendRequest()
                retryFunc (attempt + 1)
            else None
    retryFunc(0)

let sendCommand<'RequestArgs> (requestArgs: 'RequestArgs) (command: Command<'RequestArgs>): unit =
    use requester = command.NewRequester()
    requester.SendRequest requestArgs
    let rec retryFunc (attempt: int) = 
        try 
            Util.Log.debugInfo $"Sending new request for command {command.QueueName}"
            let task = Async.AwaitEvent requester.NewResponse
            let response = Async.RunSynchronously(task, responseTimeoutMilliseconds)
            match response with
            | MessageQueueCommand.Response.Success -> ()
            | MessageQueueCommand.Response.Fail errorDetails -> Util.Log.error errorDetails
        with error ->
            Util.Log.error $"Response failed {command.QueueName}. {error.Message}"
            if attempt < responseMaxRetries then 
                Util.Log.error $"Retrying {command.QueueName}"
                requester.SendRequest requestArgs
                retryFunc (attempt + 1)
    retryFunc(0)

let bindCommand<'RequestArgs> 
    (handleFunct: 'RequestArgs -> unit) 
    (command: MessageQueueCommand.Command<'RequestArgs>) =
    let currentExecutableFilePath = Util.Reflection.currentExecutableFilePath()
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
    let currentExecutableFilePath = Util.Reflection.currentExecutableFilePath()
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
