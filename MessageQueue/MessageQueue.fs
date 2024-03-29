module Util.MessageQueue

open Util.Path
open System.IO

exception QueueLimitReachedException of string

let tempDirPath = Util.Environment.SpecialFolder.temporary/DirectoryName "cb045d48-ac03-4dc2-b5d2-565aa32e70af"
let queueLimit = 1000

let getQueueDirPath queueName =
    tempDirPath/DirectoryPath queueName

let getQueueHeadFilePath queueName =
    let queueDirPath = getQueueDirPath queueName
    queueDirPath/FileName "head"

let getQueueTailFilePath queueName =
    let queueDirPath = getQueueDirPath queueName
    queueDirPath/FileName "tail"

let getQueueLockFilePath queueName = 
    let queueDirPath = getQueueDirPath queueName
    queueDirPath/FileName "lock"

let getMessageFilePath queueName (index: int) =
    let queueDirPath = getQueueDirPath queueName
    let fileName = $"{index}.msg" |> FileName
    queueDirPath/fileName

let isQueueInitialized queueName = 
    let queueDirPath = getQueueDirPath queueName
    let headFilePath = getQueueHeadFilePath queueName
    let tailFilePath = getQueueTailFilePath queueName
    if Util.IO.Directory.exists queueDirPath &&
       Util.IO.File.exists headFilePath &&
       Util.IO.File.exists tailFilePath then true
    else false

let rec resetQueue queueName = 
    try
        let queueDirPath = getQueueDirPath queueName
        Util.IO.Directory.delete queueDirPath
        let queueDirPath = getQueueDirPath queueName
        Util.IO.Directory.create queueDirPath
        let headFilePath = getQueueHeadFilePath queueName
        Util.IO.File.writeText headFilePath "0"
        let tailFilePath = getQueueTailFilePath queueName
        Util.IO.File.writeText tailFilePath "0"
    with
        | :? System.IO.IOException -> 
            System.Threading.Thread.Sleep 50
            resetQueue queueName

let ensureQueueInitialized queueName = if not (queueName |> isQueueInitialized) then resetQueue queueName

let initQueue queueName = 
    ensureQueueInitialized queueName
    queueName

let listQueueTree queueName =
    let queueDirPath = getQueueDirPath queueName
    if not (Util.IO.Directory.exists queueDirPath) then Seq.empty
    else
        Util.IO.Directory.listDirectories queueDirPath
        |> Seq.map (fun x -> x.Value |> String.remove tempDirPath.Value |> String.removeFirstCharacter )
        |> Seq.filter isQueueInitialized

let rec removeQueueAsync queueName = async {
    try
        let queueDirPath = getQueueDirPath queueName
        Util.IO.Directory.delete queueDirPath
    with
        | :? System.IO.IOException -> // this is expected in multi process case
            if not (isQueueInitialized queueName) then ()  // this is expected in multi process case
            else 
                Util.Log.debugError $"remove queue error {queueName}"
                do! Async.Sleep 50
                do! removeQueueAsync queueName }

let removeQueue queueName = removeQueueAsync queueName |> Async.RunSynchronously

let countMessagesAsync queueName = async {
    let lockFilePath = getQueueLockFilePath queueName
    use lockStream = new System.IO.FileStream(lockFilePath.Value, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
    lockStream.Lock(0 |> int64, 0 |> int64)

    let headFilePath = getQueueHeadFilePath queueName
    use headStream = new System.IO.FileStream(headFilePath.Value, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
    headStream.Lock(0 |> int64, 0 |> int64)
    use headReader = new StreamReader(headStream)
    let! headIndex = headReader.ReadLineAsync() |> Async.AwaitTask
    let headIndex = headIndex |> int

    let tailFilePath = getQueueTailFilePath queueName 
    use tailStream = new System.IO.FileStream(tailFilePath.Value, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
    tailStream.Lock(0 |> int64, 0 |> int64)
    use tailReader = new StreamReader(tailStream)
    let! tailIndex = tailReader.ReadLineAsync() |> Async.AwaitTask
    let tailIndex = tailIndex |> int

    return System.Math.Abs(headIndex - tailIndex) }

let countMessages queueName = countMessagesAsync queueName |> Async.RunSynchronously

let unsafeEnqueueAsync queueName (message: string) = async {
    let lockFilePath = getQueueLockFilePath queueName
    use lockStream = new System.IO.FileStream(lockFilePath.Value, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
    lockStream.Lock(0 |> int64, 0 |> int64)

    let tailFilePath = getQueueTailFilePath queueName
    use tailStream = new System.IO.FileStream(tailFilePath.Value, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
    tailStream.Lock(0 |> int64, 0 |> int64)
    use tailReader = new StreamReader(tailStream)
    let! tailIndex = tailReader.ReadLineAsync() |> Async.AwaitTask
    let tailIndex = tailIndex |> int

    let messageFilePath = getMessageFilePath queueName tailIndex
    use messageStream = new System.IO.FileStream(messageFilePath.Value, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
    use messageWriter = new StreamWriter(messageStream)
    do! messageWriter.WriteAsync(message) |> Async.AwaitTask

    let headFilePath = getQueueHeadFilePath queueName
    use headStream = new System.IO.FileStream(headFilePath.Value, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
    headStream.Lock(0 |> int64, 0 |> int64)
    use headReader = new StreamReader(headStream)
    let! headIndex = headReader.ReadLineAsync() |> Async.AwaitTask
    let headIndex = headIndex |> int

    let newTailIndex =
        if tailIndex + 1 > queueLimit then 0
        else tailIndex + 1
    if newTailIndex = headIndex then 
        let errorMessage = sprintf "Queue limit reached in queue %s" queueName
        raise (QueueLimitReachedException errorMessage)
    let newPos = tailStream.Seek(0 |> int64, SeekOrigin.Begin)
    use tailWriter = new StreamWriter(tailStream)
    do! tailWriter.WriteLineAsync(newTailIndex |> string) |> Async.AwaitTask  }

let rec enqueueAsync queueName (message: string) = async {
    try
        Util.Log.debugInfo $"try enqueue {queueName} {message}"
        do! unsafeEnqueueAsync queueName message
        Util.Log.debugInfo $"enqueue success {queueName} {message}"
    with
        | :? System.IO.IOException -> // this is expected in multi process case
            if not (isQueueInitialized queueName) then ()
            else
                Util.Log.debugError $"enqueue error {queueName} {message}"
                do! Async.Sleep 50
                do! enqueueAsync queueName message }

let enqueue queueName (message: string) =
    enqueueAsync queueName message |> Async.RunSynchronously

let hasMessages queueName = getQueueDirPath queueName |> Util.IO.Directory.countFiles > 3

let unsafeDequeueAsync queueName = async {
    let queueDirPath = getQueueDirPath queueName

    let lockFilePath = queueDirPath/FileName "lock"
    use lockStream = new System.IO.FileStream(lockFilePath.Value, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
    lockStream.Lock(0 |> int64, 0 |> int64)
    
    let headFilePath = getQueueHeadFilePath queueName
    let tailFilePath = getQueueTailFilePath queueName

    use headStream = new System.IO.FileStream(headFilePath.Value, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
    headStream.Lock(0 |> int64, 0 |> int64)
    use headReader = new StreamReader(headStream)
    let! headIndex = headReader.ReadLineAsync() |> Async.AwaitTask
    let headIndex = headIndex |> int

    let messageFilePath = getMessageFilePath queueName headIndex
    if not (Util.IO.File.exists messageFilePath) then return None
    else 
        use messageStream = new System.IO.FileStream(messageFilePath.Value, FileMode.Open, FileAccess.Read, FileShare.None)
        use messageReader = new StreamReader(messageStream)
        let! message = messageReader.ReadToEndAsync() |> Async.AwaitTask
        Util.IO.File.delete messageFilePath

        use tailStream = new System.IO.FileStream(tailFilePath.Value, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
        tailStream.Lock(0 |> int64, 0 |> int64)
        use tailReader = new StreamReader(tailStream)
        let! tailIndex = tailReader.ReadLineAsync() |> Async.AwaitTask 
        let tailIndex = tailIndex |> int

        let newHeadIndex =
            if headIndex = tailIndex then headIndex
            elif headIndex + 1 > queueLimit then 0
            else headIndex + 1
        let newPos = headStream.Seek(0 |> int64, SeekOrigin.Begin)
        use headWriter = new StreamWriter(headStream)
        do! headWriter.WriteLineAsync(newHeadIndex |> string) |> Async.AwaitTask
        return Some message }

let rec dequeueAsync queueName =
    Util.Log.debugInfo $"dequeue {queueName}" 
    let rec retryFunc attempt =  async {
        try
            return! unsafeDequeueAsync queueName
        with
        | :? System.IO.IOException -> // this is expected in multi process case
            if not (isQueueInitialized queueName) then return None  // this is expected in multi process case
            else
                Util.Log.debugError $"dequeue error {queueName}"
                if attempt < 10 then
                    do! Async.Sleep 50
                    return! retryFunc (attempt + 1)
                else return None }
    retryFunc 0

let dequeue queueName =
    dequeueAsync queueName |> Async.RunSynchronously

let dequeueAllAsync queueName = async {
    let messages = System.Collections.Generic.List<string>()
    while hasMessages queueName do 
        match! dequeueAsync queueName with
        | Some message -> messages.Add message
        | None -> ()
    return messages |> List.ofSeq }

let persist queueName destinationPath = async {
    let queueDirPath = getQueueDirPath queueName
    Util.IO.Directory.copy queueDirPath destinationPath }

let rec periodicPersistence queueNames destinationDirPath (timeSpan: System.TimeSpan) = async {
    let sleepTime = timeSpan.TotalMilliseconds |> int
    do! Async.Sleep sleepTime
    queueNames
    |> Seq.map(persist destinationDirPath)
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
    do! periodicPersistence queueNames destinationDirPath timeSpan }

type Queue<'MessageType>(queueName) =
    do
        ensureQueueInitialized queueName
    member val QueueName = queueName
    member this.EnqueueAsync (message: 'MessageType) = async {
        let messageString = Util.Json.toJson message
        do! enqueueAsync queueName messageString }
    member this.Enqueue (message: 'MessageType) = 
        this.EnqueueAsync message |> Async.RunSynchronously
    member this.DequeueAsync () = async {
        match! dequeueAsync queueName with
        | Some messageString ->
            let message = Util.Json.fromJson<'MessageType> messageString
            return Some message
        | None -> return None }
    member this.Dequeue () =
        this.DequeueAsync() |> Async.RunSynchronously
    member this.DequeueAllAsync () = async {
        let! stringMessages = dequeueAllAsync queueName
        let messages = stringMessages |> List.map Util.Json.fromJson<'MessageType>
        return messages }
