module Util.Service.MessageQueueTreeMonitor

type Config = {
    QueueName: string
    UpdateRate: System.TimeSpan
    ResetQueue: bool }

type Message = {
    Content: string }

type Events = {
    NewMessage: IEvent<SubQueueMessage> }
and SubQueueMessage = { SubQueueName: string; Message: Message }

let init (config: Config) =
    if config.ResetQueue then Util.MessageQueue.resetQueue config.QueueName
    let newMessageEvent = new Event<SubQueueMessage>()
    let events = { Events.NewMessage = newMessageEvent.Publish }
    let task = Util.Service.Daemon.init config.UpdateRate (fun _ -> async {
        Util.MessageQueue.listQueueTree config.QueueName
        |> Seq.map(fun subQueue -> async {
            let! messageContent = Util.MessageQueue.dequeueAsync subQueue
            if messageContent <> "" then 
                let subQueueMessage = {
                    SubQueueMessage.SubQueueName = subQueue
                    Message = { Message.Content = messageContent } }
                newMessageEvent.Trigger subQueueMessage } )
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore } )
    (task, events)

let waitForMessage (config: Config) =
    let service, events = init config
    service |> Async.Start
    events.NewMessage |> Async.AwaitEvent |> Async.RunSynchronously
