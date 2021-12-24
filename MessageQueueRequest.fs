module Util.MessageQueueRequest

open Util.Service.MessageQueueMonitor
open Util.Json

type WriteRequest<'a> (queueName) =
    let requestQueueName = $"{queueName}/request"
    do Util.MessageQueue.ensureQueueInitialized requestQueueName
    member this.QueueName = requestQueueName
    member this.SendRequest message = message |> toJson |> Util.MessageQueue.enqueue requestQueueName
    member this.ReadRequest () =
        let message = Util.MessageQueue.dequeue requestQueueName
        if message <> "" then message |> fromJson |> Some
        else None
    member this.Subscribe () =
        let newRequestEvent = new Event<'a>()
        let events = { WriteEvents.NewRequest = newRequestEvent.Publish  }
        let messageQueueConfig = {
            Util.Service.MessageQueueMonitor.Config.QueueName = requestQueueName
            UpdateRate = System.TimeSpan.FromSeconds(1.0)
            ResetQueue = false }
        let task, messageQueueEvents = Util.Service.MessageQueueMonitor.init messageQueueConfig
        messageQueueEvents.NewMessage.Add(fun message -> 
            fromJson message.Content
            |> newRequestEvent.Trigger )
        (task, events)
and WriteEvents<'a> = { NewRequest: IEvent<'a> }

type Response<'a> (queueName) =
    let responseQueueName = $"{queueName}/response"
    do Util.MessageQueue.ensureQueueInitialized responseQueueName
    member this.Subscribe () =
        let newResponseEvent = new Event<'a>()
        let events = { ResponseEvents.NewResponse = newResponseEvent.Publish  }
        let messageQueueConfig = {
            Util.Service.MessageQueueMonitor.Config.QueueName = responseQueueName
            UpdateRate = System.TimeSpan.FromSeconds(1.0)
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

type ReadRequest<'a, 'b> (queueName) =
    let requestQueueName = $"{queueName}/request"
    do Util.MessageQueue.ensureQueueInitialized requestQueueName
    member this.QueueName = requestQueueName
    member this.SendRequest message = 
        message |> toJson |> Util.MessageQueue.enqueue requestQueueName
        Response<'b>(queueName)
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
            UpdateRate = System.TimeSpan.FromSeconds(1.0)
            ResetQueue = false }
        let task, messageQueueEvents = Util.Service.MessageQueueMonitor.init messageQueueConfig
        messageQueueEvents.NewMessage.Add(fun message -> 
            let apiMetaData = fromJson message.Content
            newRequestEvent.Trigger apiMetaData )
        (task, events)
and ReadEvents<'a> = { NewRequest: IEvent<'a> }