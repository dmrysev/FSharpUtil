module Util.MessageQueueCommand

open Util.Service.MessageQueueMonitor
open Util.Service.MessageQueueTreeMonitor
open Util.Json

type Command<'a> (queueName: string, ?config: Config) =
    let config = defaultArg config { 
        ListenerUpdareRate = System.TimeSpan.FromMilliseconds(100)
        ResetQueue = false }
    let requestQueueName = $"{queueName}/request"
    let parseMessage message =
        match message with
        | Some message -> message |> fromJson |> Some
        | None -> None    
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
        let events = { Events.NewRequest = newRequestEvent.Publish  }
        let messageQueueConfig = {
            Util.Service.MessageQueueMonitor.Config.QueueName = requestQueueName
            UpdateRate = config.ListenerUpdareRate }
        let task, messageQueueEvents = Util.Service.MessageQueueMonitor.init messageQueueConfig
        messageQueueEvents.NewMessage.Add(fun message -> 
            fromJson message.Content
            |> newRequestEvent.Trigger )
        (task, events)
and Events<'a> = { NewRequest: IEvent<'a> }
and Config = { 
    ListenerUpdareRate: System.TimeSpan
    ResetQueue: bool }
