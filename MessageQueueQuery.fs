module Util.MessageQueueQuery

open Util.Service.MessageQueueMonitor
open Util.Service.MessageQueueTreeMonitor
open Util.Json

type Query<'a, 'b> (queueName: string, ?config: Config) =
    let config = defaultArg config { ListenerUpdateRate = System.TimeSpan.FromSeconds(1.0) }
    member this.ResetQueue() = Util.MessageQueue.removeQueue queueName
    member this.SendRequest message = 
        let recieverId = Util.Guid.generate()
        let requestQueueName = $"{queueName}/request/{recieverId}"
        Util.MessageQueue.ensureQueueInitialized requestQueueName
        message |> toJson |> Util.MessageQueue.enqueue requestQueueName
        new ResponseHandler<'b>(queueName, recieverId, config)
    member this.Subscribe () =
        let newRequestEvent = new Event<QueryHandler<'a, 'b>>()
        let events = { QueryEvents.NewRequest = newRequestEvent.Publish  }
        let messageQueueConfig = {
            Util.Service.MessageQueueTreeMonitor.Config.QueueName = $"{queueName}/request"
            UpdateRate = config.ListenerUpdateRate
            ResetQueue = false }
        let task, messageQueueEvents = Util.Service.MessageQueueTreeMonitor.init messageQueueConfig
        messageQueueEvents.NewMessage.Add(fun subQueueMessage -> 
            QueryHandler(queueName, subQueueMessage) |> newRequestEvent.Trigger )
        (task, events)
and QueryEvents<'a, 'b> = { NewRequest: IEvent<QueryHandler<'a, 'b>> }
and QueryHandler<'a, 'b>(queueName, subQueueMessage: SubQueueMessage) =
    let recieverId = subQueueMessage.SubQueueName |> String.split "/" |> Seq.last
    member this.RequestMessage = subQueueMessage.Message.Content |> fromJson<'a>
    member this.SendResponse(message: 'b) = 
        let responseQueueName = $"{queueName}/response/{recieverId}"
        Util.MessageQueue.ensureQueueInitialized responseQueueName
        message |> toJson |> Util.MessageQueue.enqueue responseQueueName
        ()
and Config = { ListenerUpdateRate: System.TimeSpan }
and ResponseHandler<'b> (queueName, recieverId, config: Config) =
    let requestQueueName = $"{queueName}/request/{recieverId}"
    let responseQueueName = $"{queueName}/response/{recieverId}"
    do Util.MessageQueue.ensureQueueInitialized responseQueueName
    interface System.IDisposable with
        member this.Dispose() = 
            Util.MessageQueue.removeQueue requestQueueName
            Util.MessageQueue.removeQueue responseQueueName
    member this.Subscribe () =
        let newResponseEvent = new Event<'b>()
        let events = { ResponseEvents.NewResponse = newResponseEvent.Publish  }
        let messageQueueConfig = {
            Util.Service.MessageQueueMonitor.Config.QueueName = responseQueueName
            UpdateRate = config.ListenerUpdateRate }
        let task, messageQueueEvents = Util.Service.MessageQueueMonitor.init messageQueueConfig
        messageQueueEvents.NewMessage.Add(fun message -> 
            fromJson<'b> message.Content
            |> newResponseEvent.Trigger )
        (task, events)
    member this.WaitForOne () =
        let task, events = this.Subscribe()
        use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
        Async.Start(task, taskCancellation.Token)
        Async.AwaitEvent events.NewResponse |> Async.RunSynchronously
and ResponseEvents<'b> = { NewResponse: IEvent<'b> }
