module Util.Service.MessageQueueMonitor

open System.IO
type T (config: Config) =
    do
        Util.MessageQueue.ensureQueueInitialized config.QueueName
    let newMessageEvent = new Event<Message>()
    let checkQueue() =
        Util.Log.debugInfo $"queue monitor check queue {config.QueueName}"
        let result = Util.MessageQueue.dequeue config.QueueName
        match result with
        | Some content ->
            Util.Log.debugInfo $"queue monitor new content {content} {config.QueueName}"
            let message: Message = {
                Content = content }
            newMessageEvent.Trigger message
        | None -> ()
    let watcher, subscriber = 
        let queueDirPath = Util.MessageQueue.getQueueDirPath config.QueueName
        let watcher = new System.IO.FileSystemWatcher (queueDirPath.Value)
        watcher.NotifyFilter <- NotifyFilters.LastWrite
        watcher.Filter <- "*.msg"
        watcher.IncludeSubdirectories <- false
        watcher.EnableRaisingEvents <- true
        let subscriber = watcher.Changed |> Observable.subscribe (fun (e: FileSystemEventArgs) -> 
            Util.Log.debugInfo $"queue monitor watcher changed {queueDirPath.Value}"
            checkQueue() )
        watcher.Error.Add(fun e -> 
            let ex = e.GetException()
            Util.Log.debugError "tree monitor watcher error {queueDirPath.Value}. {ex.Message}" )
        watcher, subscriber
    interface System.IDisposable with
        member this.Dispose() = 
            subscriber.Dispose()
            watcher.Dispose()
    member val NewMessage = newMessageEvent.Publish with get
    member this.CheckQueue() = checkQueue()
    
and Config = { QueueName: string }
and Message = { Content: string }
and Events = { NewMessage: IEvent<Message> }
