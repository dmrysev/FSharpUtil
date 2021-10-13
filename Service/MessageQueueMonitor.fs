module Util.Service.MessageQueueMonitor

type Config = {
    QueueName: string
    UpdateRate: System.TimeSpan
    ResetQueue: bool }

type Message = {
    Content: string }

type Events = {
    NewMessage: IEvent<Message> }

let init (config: Config) =
    if config.ResetQueue then Util.MessageQueue.resetQueue config.QueueName
    let updateRateMs = config.UpdateRate.TotalMilliseconds |> int
    let newMessageEvent = new Event<Message>()
    let events = { Events.NewMessage = newMessageEvent.Publish }
    let rec service () = async {
        let! messageContent = Util.MessageQueue.dequeueAsync config.QueueName
        if messageContent <> "" then 
            let message = {
                Message.Content = messageContent }
            newMessageEvent.Trigger message
        do! Async.Sleep updateRateMs
        do! service() }
    (service(), events)

let waitForMessage (config: Config) =
    let service, events = init config
    service |> Async.Start
    events.NewMessage |> Async.AwaitEvent |> Async.RunSynchronously
