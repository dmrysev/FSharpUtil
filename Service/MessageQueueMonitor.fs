module Util.Service.MessageQueueMonitor

type Config = {
    QueueName: string
    UpdateRate: System.TimeSpan }

type Message = {
    Content: string }

type Events = {
    NewMessage: IEvent<Message> }

let init (config: Config) =
    Util.MessageQueue.ensureQueueInitialized config.QueueName
    let newMessageEvent = new Event<Message>()
    let events = { Events.NewMessage = newMessageEvent.Publish }
    let task = Util.Service.Daemon.init config.UpdateRate (fun _ -> async {
        let! messageContent = Util.MessageQueue.dequeueAsync config.QueueName
        if messageContent <> "" then 
            let message = {
                Message.Content = messageContent }
            newMessageEvent.Trigger message })
    (task, events)

let waitForMessage (config: Config) =
    let service, events = init config
    service |> Async.Start
    events.NewMessage |> Async.AwaitEvent |> Async.RunSynchronously
