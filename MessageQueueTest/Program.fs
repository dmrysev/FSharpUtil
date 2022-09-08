open Util.MessageQueueQuery
open Util.IO.Path
open System
open System.IO

let test1() =
    let timeout = TimeSpan.FromMilliseconds 100
    let timeoutMilliseconds = timeout.TotalMilliseconds |> int
    // let test i = 
    //     while true do
    //         printfn $"{i}"
    //         // Util.IO.Directory.countFiles (DirectoryPath "/tmp/cb045d48-ac03-4dc2-b5d2-565aa32e70af/media/api/download_status/new_status/request") |> ignore 
    //         do! Async.Sleep timeoutMilliseconds
    //         // Threading.Thread.Sleep timeoutMilliseconds
    let test i = async { 
        do! Async.Sleep 10
        while true do
            // printfn $"{i}"
            Util.IO.Directory.exists (DirectoryPath "/tmp/cb045d48-ac03-4dc2-b5d2-565aa32e70af/media/api/download_status/new_status/request") |> ignore 
            do! Async.Sleep timeoutMilliseconds
            // Threading.Thread.Sleep timeoutMilliseconds
    }
    test 
    |> Seq.replicate 300
    |> Seq.mapi (fun i task -> task i)
    // |> PSeq.iteri (fun i task -> task i)
    |> Async.Parallel
    |> Async.Ignore
    |> Async.RunSynchronously
    // |> ignore


let testWatcher() =
    let initWatcher i = 
        let dirPath = DirectoryPath $"/tmp/watcher_test/{i}"
        Util.IO.Directory.create dirPath
        let watcher = new System.IO.FileSystemWatcher(dirPath.Value)
        watcher.NotifyFilter <- NotifyFilters.LastWrite
        watcher.Filter <- ""
        watcher.IncludeSubdirectories <- true
        watcher.EnableRaisingEvents <- true
        watcher.Changed.Add (fun (e: FileSystemEventArgs) -> 
            printfn $"changed {i}")
        watcher

    let initWriter i = async {
        let dirPath = DirectoryPath $"/tmp/watcher_test/{i}"
        Util.IO.Directory.create dirPath
        let timeout = TimeSpan.FromMilliseconds 100
        while true do
            let fileName = sprintf "%07i" i |> FileName
            let filePath = dirPath/fileName
            Util.IO.File.create filePath
            do! Util.Async.sleep timeout }
    initWriter 0 |> Async.Start
    initWriter 1 |> Async.Start
    let watchers =
        initWatcher
        |> Seq.replicate 20
        |> Seq.mapi (fun i func -> func i)
        |> Seq.toArray
    while true do ()


let testQuery arg0 =
    let printResponse (response: Response<string>) =
        match response with
        | Success message -> printfn "%s" message
        | Fail error -> printfn "%A" error        
    let query = Util.MessageQueueQuery.Query<string, string>("test/111")
    let input = arg0 |> int
    match input with
    | 0 -> 
        let mutable count = 0
        query.NewResponder().NewRequest.Add(fun requestHandler -> Async.Start(async {
            // printfn $"{requestHandler.RequestMessage}"
            while true do
                requestHandler.SendResponse $"response for {requestHandler.RequestArgs} {count}"
                count <- count + 1
                do! Async.Sleep 1 }))
        System.Console.ReadLine() |> ignore
    | 1 -> query.SingleResponseRequest $"test request id={input}" |> ignore
    | 2 ->
        let mutable count = 0
        use responseHandler = query.NewRequester()
        responseHandler.NewResponse.Add(fun response -> 
            match response with
            | Success message ->
                count <- count + 1
                if count % 100 = 0 then printfn "%s %i" message count
            | Fail error -> printfn "%A" error )
        responseHandler.SendRequest $"test request id={input}"
        System.Console.ReadLine() |> ignore
    | 3 ->
        query.SingleResponseRequest $"test request id={input}" |> printResponse
        System.Console.ReadLine() |> ignore
    | 4 -> 
        let mutable count = 0
        let initRequester i = async {
            while true do
                count <- count + 1
                let maxAttempts = 3
                let rec func (attempt: int) =
                    try 
                        printfn $"send request id={i} count={count}"
                        query.SingleResponseRequest $"request id={i} count={count}" |> printResponse
                    with error ->
                        printfn "%s" error.Message
                        if attempt < 3 then func (attempt + 1)
                        else raise error  
                func 0
                do! Async.Sleep 100
            }
        initRequester arg0 |> Async.RunSynchronously
    | 5 -> 
        let mutable count = 0
        query.NewResponder().NewRequest.Add(fun requestHandler ->
            printfn $"{requestHandler.RequestArgs}"
            requestHandler.SendResponse $"response {count}"
            count <- count + 1 )
        query.SingleResponseRequest $"test request id={input}" |> printResponse
    |6 -> 
        let mutable count = 0
        use requestListener = query.NewResponder()
        requestListener.NewRequest.Add(fun requestHandler ->
            // printfn $"{requestHandler.RequestMessage}"
            requestHandler.SendResponse $"response for {requestHandler.RequestArgs}"
            count <- count + 1 )
        System.Console.ReadLine() |> ignore
    | 8 -> 
        let mutable count = 0
        let initRequester i = async {
            while true do
                printfn $"send request id={i} count={count}"
                query.SingleResponseRequest $"request id={i} count={count}" |> printResponse
                count <- count + 1 }
        initRequester arg0 |> Async.RunSynchronously
    | 9 -> 
        let mutable count = 0
        let initRequester i = async {
            while true do
                // printfn $"send request id={i} count={count}"
                let response = query.SingleResponseRequest $"request id={i} count={count}"
                if count % 10 = 0 then response |> printResponse
                count <- count + 1 }
        initRequester arg0 |> Async.RunSynchronously
    | 10 ->
        query.SingleResponseRequest $"test request id={input}" |> printResponse
    | 11 ->
        let mutable count = 0
        while true do
            query.SingleResponseRequest $"test request id={input} cout={count}" |> printResponse
            count <- count + 1
            System.Threading.Thread.Sleep 50
    | _ -> ()

let testMonitorTree arg0 =
    let config: Util.Service.MessageQueueTreeMonitor.Config = { QueueName = "test/222"; UpdateRate = (System.TimeSpan.FromMilliseconds 100); ResetQueue = false}
    if arg0 = "0" then
        use monitor = new Util.Service.MessageQueueTreeMonitor.T (config)
        monitor.NewMessage.Add(fun m -> printfn "%s" m.Content)
        System.Console.ReadLine() |> ignore
    else 
        let responseQueueName = "test/222/1"
        Util.MessageQueue.ensureQueueInitialized responseQueueName
        Util.MessageQueue.enqueue responseQueueName "blah"

let testQueueMonitor arg0 =
    let input = arg0 |> int
    match input with
    | 0 -> 
        let config1: Util.Service.MessageQueueMonitor.Config = {
            QueueName = "test/1"
            UpdateRate = (TimeSpan.FromMilliseconds 100) }
        use monitor1 = new Util.Service.MessageQueueMonitor.T (config1)
        monitor1.NewMessage.Add (fun message -> 
            printfn "%s" message.Content
            System.Threading.Thread.Sleep 3000 )
        let config2: Util.Service.MessageQueueMonitor.Config = {
            QueueName = "test/2"
            UpdateRate = (TimeSpan.FromMilliseconds 100) }
        use monitor2 = new Util.Service.MessageQueueMonitor.T (config2)
        monitor2.NewMessage.Add (fun message -> 
            printfn "%s" message.Content
            System.Threading.Thread.Sleep 1000 )
        System.Console.ReadLine() |> ignore
    | 1 -> 
        while true do
            Util.MessageQueue.enqueue "test/1" "hello 1"
            System.Threading.Thread.Sleep 1000
    | 2 -> 
        while true do
            Util.MessageQueue.enqueue "test/2" "hello 2"
            System.Threading.Thread.Sleep 500
    | _ -> ()

[<EntryPoint>]
let main argv =
    printfn "Started"
    testQueueMonitor argv[0]
    0