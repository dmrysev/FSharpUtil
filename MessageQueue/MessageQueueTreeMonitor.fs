module Util.Service.MessageQueueTreeMonitor

open System.IO

type T (config: Config) =
    let newMessageEvent = new Event<SubQueueMessage>()
    let checkQueue() =
        Util.MessageQueue.listQueueTree config.QueueName
        |> Seq.iter(fun subQueue ->
            Util.Log.debugInfo $"queue tree monitor, dequeue sub queue {subQueue}"
            let result = Util.MessageQueue.dequeue subQueue
            match result with
            | Some content ->
                let message: SubQueueMessage = {
                    SubQueueName = subQueue
                    Content = content }
                newMessageEvent.Trigger message
            | None -> () )
    let watcher, subscriber = 
        Util.MessageQueue.ensureQueueInitialized config.QueueName
        let queueDirPath = Util.MessageQueue.getQueueDirPath config.QueueName
        let watcher = new System.IO.FileSystemWatcher (queueDirPath.Value)
        watcher.NotifyFilter <- NotifyFilters.LastWrite
        watcher.Filter <- "*.msg"
        watcher.IncludeSubdirectories <- true
        watcher.EnableRaisingEvents <- true
        let subscriber = watcher.Changed |> Observable.subscribe (fun (e: FileSystemEventArgs) -> 
            Util.Log.debugInfo $"tree monitor watcher changed {queueDirPath.Value}"
            checkQueue() )
        watcher.Error.Add(fun e -> 
            let ex = e.GetException()
            Util.Log.debugError $"tree monitor watcher error {queueDirPath.Value}. {ex.Message}" )
        watcher, subscriber
    interface System.IDisposable with
        member this.Dispose() = 
            subscriber.Dispose()
            watcher.Dispose()
    member val NewMessage = newMessageEvent.Publish with get

and Config = { QueueName: string; ResetQueue: bool }
and Message = { Content: string }
and Events = { NewMessage: IEvent<SubQueueMessage> }
and SubQueueMessage = { SubQueueName: string; Content: string }
