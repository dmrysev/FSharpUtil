module Util.MessageQueueQuery

open Util.Service.MessageQueueMonitor
open Util.Service.MessageQueueTreeMonitor
open Util.Json

type Query<'RequestArgs, 'ResponseMessage> (queueName: string, ?config: Config) =
    let config = defaultArg config Config.Default
    member val QueueName = queueName with get
    member this.NewResponder() = new Responder<'RequestArgs, 'ResponseMessage> (queueName)
    member this.SingleResponseRequest requestArgs =
        let recieverId = Util.Guid.generate()
        let requestQueueName = $"{queueName}/request/{recieverId}"
        Util.MessageQueue.ensureQueueInitialized requestQueueName
        use requester = new Requester<'RequestArgs, 'ResponseMessage>(queueName, recieverId, config.ResponseTimeout, config.ResponseMaxRetries)
        requester.SendRequest requestArgs
        requester.WaitForOne()
    member this.NewRequester() = 
        let recieverId = Util.Guid.generate()
        let requestQueueName = $"{queueName}/request/{recieverId}"
        Util.MessageQueue.ensureQueueInitialized requestQueueName
        new Requester<'RequestArgs, 'ResponseMessage>(queueName, recieverId, config.ResponseTimeout, config.ResponseMaxRetries)

and Config = {
    ResponseTimeout: System.TimeSpan option
    ResponseMaxRetries: int option }
    with static member Default = {
            ResponseTimeout = None
            ResponseMaxRetries = None }

and Requester<'RequestArgs, 'ResponseMessage> (
        queueName: string, recieverId: string, 
        responseTimeout: System.TimeSpan option, responseMaxRetries: int option) =
    let requestQueueName = $"{queueName}/request/{recieverId}"
    let responseQueueName = $"{queueName}/response/{recieverId}"
    let responseTimeoutMilliseconds = 
        let timeout = responseTimeout |> Option.defaultValue (System.TimeSpan.FromMinutes 1.0)
        timeout.TotalMilliseconds |> int
    let responseMaxRetries = responseMaxRetries |> Option.defaultValue 3
    let newResponseEvent = new Event<Response<'ResponseMessage>>()
    let mutable currentRequestArgs: 'RequestArgs option = None
    let messageQueueMonitor, subscriber =
        Util.MessageQueue.ensureQueueInitialized responseQueueName
        let messageQueueConfig: Util.Service.MessageQueueMonitor.Config = { QueueName = responseQueueName }
        let monitor = new Util.Service.MessageQueueMonitor.T (messageQueueConfig)
        let subscriber = monitor.NewMessage |> Observable.subscribe (fun message -> 
            Util.Log.debugInfo $"query new message {message}"
            fromJson<Response<'ResponseMessage>> message.Content
            |> newResponseEvent.Trigger )    
        monitor, subscriber
    interface System.IDisposable with
        member this.Dispose() = 
            subscriber.Dispose()
            (messageQueueMonitor :> System.IDisposable).Dispose()
            Util.MessageQueue.removeQueue requestQueueName
            Util.MessageQueue.removeQueue responseQueueName
    member this.SendRequest (requestArgs: 'RequestArgs) =
        currentRequestArgs <- Some requestArgs
        requestArgs |> toJson |> Util.MessageQueue.enqueue requestQueueName
    member val NewResponse = newResponseEvent.Publish
    member this.WaitForOne () =
        let rec retryFunc (attempt: int) =
            try 
                let task = Async.AwaitEvent this.NewResponse
                Async.RunSynchronously(task, responseTimeoutMilliseconds)
            with error ->
                Util.Log.debugError $"Response failed {responseQueueName}. {error.Message}"
                if attempt < responseMaxRetries then 
                    Util.Log.debugError $"Retrying {responseQueueName}"
                    // messageQueueMonitor.CheckQueue()
                    this.SendRequest currentRequestArgs.Value
                    retryFunc (attempt + 1)
                else raise error
        retryFunc(0)

and Responder<'RequestArgs, 'ResponseMessage> (queueName) =
    let newRequestEvent = new Event<RequestHandler<'RequestArgs, 'ResponseMessage>>()
    let messageQueueMonitor, subscriber =
        let requestQueueName = $"{queueName}/request"
        Util.MessageQueue.ensureQueueInitialized requestQueueName
        let messageQueueConfig: Util.Service.MessageQueueTreeMonitor.Config = {
            QueueName = $"{queueName}/request"
            ResetQueue = false }
        let monitor = new Util.Service.MessageQueueTreeMonitor.T (messageQueueConfig)
        let subscriber = monitor.NewMessage |> Observable.subscribe(fun subQueueMessage -> 
            RequestHandler<'RequestArgs, 'ResponseMessage>(queueName, subQueueMessage) |> newRequestEvent.Trigger )        
        monitor, subscriber
    interface System.IDisposable with
        member this.Dispose() = 
            subscriber.Dispose()
            (messageQueueMonitor :> System.IDisposable).Dispose()
    member val NewRequest = newRequestEvent.Publish with get

and RequestHandler<'RequestArgs, 'ResponseMessage>(queueName, subQueueMessage: Util.Service.MessageQueueTreeMonitor.SubQueueMessage) =
    let recieverId = subQueueMessage.SubQueueName |> String.split "/" |> Seq.last
    let responseQueueName = $"{queueName}/response/{recieverId}"
    member this.RequestArgs = subQueueMessage.Content |> fromJson<'RequestArgs>
    member this.SendResponse(message: 'ResponseMessage) = 
        message |> Response.Success |> toJson |> Util.MessageQueue.enqueue responseQueueName
    member this.SendFailResponse(error: Error) =
        error |> toJson |> Util.MessageQueue.enqueue responseQueueName

and Response<'ResponseMessage> = Success of 'ResponseMessage | Fail of Error
and Error = { Code: int; Responder: string; RequestName: string; Message: string }
