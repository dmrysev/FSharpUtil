module Util.MessageQueue

open Util.IO.Path
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

let getMessageFilePath queueName index =
    let queueDirPath = getQueueDirPath queueName
    let fileName = index |> string |> FileName
    queueDirPath/fileName

let isQueueInitialized queueName = 
    let queueDirPath = getQueueDirPath queueName
    let headFilePath = getQueueHeadFilePath queueName
    let tailFilePath = getQueueTailFilePath queueName
    if Util.IO.Directory.exists queueDirPath &&
       Util.IO.File.exists headFilePath &&
       Util.IO.File.exists tailFilePath then true
    else false

let resetQueue queueName = 
    let queueDirPath = getQueueDirPath queueName
    Util.IO.Directory.delete queueDirPath
    let queueDirPath = getQueueDirPath queueName
    Util.IO.Directory.create queueDirPath
    let headFilePath = getQueueHeadFilePath queueName
    Util.IO.File.writeText headFilePath "0"
    let tailFilePath = getQueueTailFilePath queueName
    Util.IO.File.writeText tailFilePath "0"

let ensureQueueInitialized queueName = if not (queueName |> isQueueInitialized) then resetQueue queueName

let listQueueTree queueName =
    let queueDirPath = getQueueDirPath queueName
    if not (Util.IO.Directory.exists queueDirPath) then Seq.empty
    else
        Util.IO.Directory.listDirectories queueDirPath
        |> Seq.map (fun x -> x.Value |> String.remove tempDirPath.Value |> String.removeFirstCharacter )

let removeQueue queueName =
    let queueDirPath = getQueueDirPath queueName
    Util.IO.Directory.delete queueDirPath

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
    do! tailWriter.WriteLineAsync(newTailIndex |> string) |> Async.AwaitTask }

let rec enqueueAsync queueName (message: string) = async {
    try
        do! unsafeEnqueueAsync queueName message
    with
        | :? System.IO.IOException -> // this is expected in multi process case
            do! Async.Sleep 50
            do! enqueueAsync queueName message }

let enqueue queueName (message: string) =
    enqueueAsync queueName message |> Async.RunSynchronously

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
    if not (Util.IO.File.exists messageFilePath) then return ""
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
        return message }

let rec dequeueAsync queueName = async {
    try
        return! unsafeDequeueAsync queueName
    with
        | :? System.IO.IOException -> // this is expected in multi process case
            do! Async.Sleep 50
            return! dequeueAsync queueName }

let dequeue queueName =
    dequeueAsync queueName |> Async.RunSynchronously

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
