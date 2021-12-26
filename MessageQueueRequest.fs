module Util.MessageQueueRequest

open Util.Service.MessageQueueMonitor
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
    let requestQueueName = $"{queueName}/request"
    do Util.MessageQueue.ensureQueueInitialized requestQueueName
    member this.QueueName = requestQueueName
    member this.SendRequest message = 
        message |> toJson |> Util.MessageQueue.enqueue requestQueueName
        Response(queueName, config)
    member this.ReadRequest () =
        let message = Util.MessageQueue.dequeue requestQueueName
        if message <> "" then message |> fromJson<'a> |> Some
        else None
    member this.SendResponse (message: 'b) = message |> toJson |> Util.MessageQueue.enqueue $"{queueName}/response"
    member this.Subscribe () =
        let newRequestEvent = new Event<'a>()
        let events = { ReadEvents.NewRequest = newRequestEvent.Publish  }
        let messageQueueConfig = {
            Util.Service.MessageQueueMonitor.Config.QueueName = requestQueueName
            UpdateRate = config.ListenerUpdareRate
            ResetQueue = false }
        let task, messageQueueEvents = Util.Service.MessageQueueMonitor.init messageQueueConfig
        messageQueueEvents.NewMessage.Add(fun message -> 
            let apiMetaData = fromJson message.Content
            newRequestEvent.Trigger apiMetaData )
        (task, events)
and ReadEvents<'a> = { NewRequest: IEvent<'a> }
and ReadRequestConfig = { ListenerUpdareRate: System.TimeSpan }
and Response<'b> (queueName, config: ReadRequestConfig) =
    let responseQueueName = $"{queueName}/response"
    do Util.MessageQueue.ensureQueueInitialized responseQueueName
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