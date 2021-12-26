module Util.MessageQueueRequest

open Util.Service.MessageQueueMonitor
open Util.Service.MessageQueueTreeMonitor
open Util.Json

type WriteRequest<'a> (queueName: string, ?config: WriteRequestConfig) =
    let config = defaultArg config { 
        ListenerUpdareRate = System.TimeSpan.FromSeconds(1.0)
        ResetQueue = false }
    let requestQueueName = $"{queueName}/request"
    let parseMessage message =
        if message <> "" then message |> fromJson |> Some
        else None    
    do if config.ResetQueue then Util.MessageQueue.resetQueue requestQueueName
        else Util.MessageQueue.ensureQueueInitialized requestQueueName
    member this.QueueName = requestQueueName
    member this.SendRequest message = message |> toJson |> Util.MessageQueue.enqueue requestQueueName
    member this.SendRequestAsync message = message |> toJson |> Util.MessageQueue.enqueueAsync requestQueueName
    member this.ReadRequest () = Util.MessageQueue.dequeue requestQueueName |> parseMessage
    member this.ReadRequestAsync () = async {
        let! message = Util.MessageQueue.dequeueAsync requestQueueName
        return parseMessage message }
    member this.Subscribe () =
        let newRequestEvent = new Event<'a>()
        let events = { WriteEvents.NewRequest = newRequestEvent.Publish  }
        let messageQueueConfig = {
            Util.Service.MessageQueueMonitor.Config.QueueName = requestQueueName
            UpdateRate = config.ListenerUpdareRate
            ResetQueue = false }
        let task, messageQueueEvents = Util.Service.MessageQueueMonitor.init messageQueueConfig
        messageQueueEvents.NewMessage.Add(fun message -> 
            fromJson message.Content
            |> newRequestEvent.Trigger )
        (task, events)
and WriteEvents<'a> = { NewRequest: IEvent<'a> }
and WriteRequestConfig = { 
    ListenerUpdareRate: System.TimeSpan
    ResetQueue: bool }

type ReadRequest<'a, 'b> (queueName: string, ?config: ReadRequestConfig) =
    let config = defaultArg config { ListenerUpdareRate = System.TimeSpan.FromSeconds(1.0) }
    member this.ResetQueue() = Util.MessageQueue.removeQueue queueName
    member this.SendRequest message = 
        let recieverId = Util.Guid.generate()
        let requestQueueName = $"{queueName}/request/{recieverId}"
        Util.MessageQueue.ensureQueueInitialized requestQueueName
        message |> toJson |> Util.MessageQueue.enqueue requestQueueName
        new ReadResponseHandler(queueName, recieverId, config)
    member this.Subscribe () =
        let newRequestEvent = new Event<ReadRequestHandler<'a, 'b>>()
        let events = { ReadEvents.NewRequest = newRequestEvent.Publish  }
        let messageQueueConfig = {
            Util.Service.MessageQueueTreeMonitor.Config.QueueName = $"{queueName}/request"
            UpdateRate = config.ListenerUpdareRate
            ResetQueue = false }
        let task, messageQueueEvents = Util.Service.MessageQueueTreeMonitor.init messageQueueConfig
        messageQueueEvents.NewMessage.Add(fun subQueueMessage -> 
            ReadRequestHandler(queueName, subQueueMessage) |> newRequestEvent.Trigger )
        (task, events)
and ReadEvents<'a, 'b> = { NewRequest: IEvent<ReadRequestHandler<'a, 'b>> }
and ReadRequestHandler<'a, 'b>(queueName, subQueueMessage: SubQueueMessage) =
    let recieverId = subQueueMessage.SubQueueName |> String.split "/" |> Seq.last
    member this.RequestMessage = subQueueMessage.Message.Content |> fromJson<'a>
    member this.SendResponse(message: 'b) = 
        let responseQueueName = $"{queueName}/response/{recieverId}"
        Util.MessageQueue.ensureQueueInitialized responseQueueName
        message |> toJson |> Util.MessageQueue.enqueue responseQueueName
        ()
and ReadRequestConfig = { ListenerUpdareRate: System.TimeSpan }
and ReadResponseHandler (queueName, recieverId, config: ReadRequestConfig) =
    let requestQueueName = $"{queueName}/request/{recieverId}"
    let responseQueueName = $"{queueName}/response/{recieverId}"
    do Util.MessageQueue.ensureQueueInitialized responseQueueName
    interface System.IDisposable with
        member this.Dispose() = 
            Util.MessageQueue.removeQueue requestQueueName
            Util.MessageQueue.removeQueue responseQueueName
    member this.Subscribe () =
        let newResponseEvent = new Event<'a>()
        let events = { ResponseEvents.NewResponse = newResponseEvent.Publish  }
        let messageQueueConfig = {
            Util.Service.MessageQueueMonitor.Config.QueueName = responseQueueName
            UpdateRate = config.ListenerUpdareRate
            ResetQueue = false }
        let task, messageQueueEvents = Util.Service.MessageQueueMonitor.init messageQueueConfig
        messageQueueEvents.NewMessage.Add(fun message -> 
            fromJson message.Content
            |> newResponseEvent.Trigger )
        (task, events)
    member this.WaitForOne () =
        let task, events = this.Subscribe()
        use taskCancellation = new Util.Async.ScopedCancellationTokenSource()
        Async.Start(task, taskCancellation.Token)
        Async.AwaitEvent events.NewResponse |> Async.RunSynchronously
and ResponseEvents<'a> = { NewResponse: IEvent<'a> }