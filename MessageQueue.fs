module Util.MessageQueue

open Util.IO.Path
open System.IO

exception QueueLimitReachedException of string

let uid = "cb045d48-ac03-4dc2-b5d2-565aa32e70af"
let tempDir = Util.Environment.SpecialFolder.temporary/uid
let queueLimit = 1000

let getQueuePath queueName =
    tempDir/queueName

let getQueueHeadPath queueName =
    let queuePath = getQueuePath queueName
    queuePath/"head"

let getQueueTailPath queueName =
    let queuePath = getQueuePath queueName
    queuePath/"tail"

let getMessagePath queueName index =
    let queuePath = getQueuePath queueName
    sprintf "%s/%i" queuePath index

let resetQueue queueName = 
    let queuePath = getQueuePath queueName
    Util.IO.Directory.delete queuePath
    Util.IO.Directory.create queuePath
    let headPath = getQueueHeadPath queueName
    System.IO.File.WriteAllText(headPath, "0")
    let tailPath = getQueueTailPath queueName
    System.IO.File.WriteAllText(tailPath, "0")

let unsafeEnqueueAsync queueName (message: string) = async {
    let queuePath = getQueuePath queueName

    let lockPath = queuePath/"lock"
    use lockStream = new System.IO.FileStream(lockPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
    lockStream.Lock(0 |> int64, 0 |> int64)

    let headPath = getQueueHeadPath queueName
    let tailPath = getQueueTailPath queuePath

    use tailStream = new System.IO.FileStream(tailPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
    tailStream.Lock(0 |> int64, 0 |> int64)
    use tailReader = new StreamReader(tailStream)
    let! tailIndex = tailReader.ReadLineAsync() |> Async.AwaitTask
    let tailIndex = tailIndex |> int

    let messagePath = getMessagePath queueName tailIndex
    use messageStream = new System.IO.FileStream(messagePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
    use messageWriter = new StreamWriter(messageStream)
    do! messageWriter.WriteAsync(message) |> Async.AwaitTask

    use headStream = new System.IO.FileStream(headPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
    headStream.Lock(0 |> int64, 0 |> int64)
    use headReader = new StreamReader(headStream)
    let! headIndex = headReader.ReadLineAsync() |> Async.AwaitTask
    let headIndex = headIndex |> int

    let newTailIndex =
        if tailIndex + 1 > queueLimit then 0
        else tailIndex + 1
    if newTailIndex = headIndex then 
        let errorMessage = sprintf "Queue limit reached in queue %s" queueName
        raise (QueueLimitReachedException(errorMessage))
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
    let queuePath = getQueuePath queueName

    let lockPath = queuePath/"lock"
    use lockStream = new System.IO.FileStream(lockPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None)
    lockStream.Lock(0 |> int64, 0 |> int64)
    
    let headPath = getQueueHeadPath queueName
    let tailPath = getQueueTailPath queuePath

    use headStream = new System.IO.FileStream(headPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
    headStream.Lock(0 |> int64, 0 |> int64)
    use headReader = new StreamReader(headStream)
    let! headIndex = headReader.ReadLineAsync() |> Async.AwaitTask
    let headIndex = headIndex |> int

    let messagePath = getMessagePath queueName headIndex
    if not <| (Util.IO.Path.exists messagePath) then return ""
    else 
        use messageStream = new System.IO.FileStream(messagePath, FileMode.Open, FileAccess.Read, FileShare.None)
        use messageReader = new StreamReader(messageStream)
        let! message = messageReader.ReadToEndAsync() |> Async.AwaitTask
        Util.IO.File.delete messagePath

        use tailStream = new System.IO.FileStream(tailPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
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

let hasMessagesAsync queueName = async {
    let queuePath = getQueuePath queueName
    let line = Util.IO.File.firstLine queuePath
    let hasContent = line <> ""
    return  hasContent }

let persist queueName destinationPath = async {
    let queuePath = getQueuePath queueName
    Util.IO.File.copy queuePath destinationPath }

let rec periodicPersistence queueNames destinationDirPath (timeSpan: System.TimeSpan) = async {
    let sleepTime = timeSpan.TotalMilliseconds |> int
    do! Async.Sleep sleepTime
    queueNames
    |> Seq.map(persist destinationDirPath)
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
    do! periodicPersistence queueNames destinationDirPath timeSpan }
