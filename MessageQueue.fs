module Util.MessageQueue

open Util.IO.Path
open System.IO

let uid = "cb045d48-ac03-4dc2-b5d2-565aa32e70af"
let tempDir = Util.Environment.SpecialFolder.temporary/uid
let queueMaxSize = 12

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

let rec enqueueAsync queueName (message: string) = async {
    let queuePath = getQueuePath queueName
    let headPath = getQueueHeadPath queueName
    let tailPath = getQueueTailPath queuePath
    try 
        // printfn "Enqueue %s " message
        use tailStream = new System.IO.FileStream(tailPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
        tailStream.Lock(0 |> int64, 0 |> int64)
        use tailReader = new StreamReader(tailStream)
        let tailIndex = tailReader.ReadLine() |> int
        let messagePath = getMessagePath queueName tailIndex
        File.WriteAllText(messagePath, message)
        use headStream = new System.IO.FileStream(headPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
        headStream.Lock(0 |> int64, 0 |> int64)
        use headReader = new StreamReader(headStream)
        let headIndex = headReader.ReadLine() |> int
        let newTailIndex =
            if tailIndex + 1 > queueMaxSize then 0
            else tailIndex + 1
        if newTailIndex = headIndex then 
            headReader.Close()
            headStream.Close()
            tailStream.Close()
            failwithf "Max size message limit reached in queue %s" queueName
        tailStream.Seek(0 |> int64, SeekOrigin.Begin)
        use tailWriter = new StreamWriter(tailStream)
        tailWriter.WriteLine(newTailIndex)
    with error ->
        printfn "%s" error.Message
        do! Async.Sleep 1000
        do! enqueueAsync queueName message
}

let rec dequeueAsync queueName = async {
    let queuePath = getQueuePath queueName
    let headPath = getQueueHeadPath queueName
    let tailPath = getQueueTailPath queuePath
    try 
        use headStream = new System.IO.FileStream(headPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
        headStream.Lock(0 |> int64, 0 |> int64)
        use headReader = new StreamReader(headStream)
        let headIndex = headReader.ReadLine() |> int
        let messagePath = getMessagePath queueName headIndex
        if not <| (Util.IO.Path.exists messagePath) then return ""
        else 
            let message = System.IO.File.ReadAllText(messagePath)
            Util.IO.File.delete messagePath
            use tailStream = new System.IO.FileStream(tailPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
            tailStream.Lock(0 |> int64, 0 |> int64)
            use tailReader = new StreamReader(tailStream)
            let tailIndex = tailReader.ReadLine() |> int        
            let newHeadIndex =
                if headIndex = tailIndex then headIndex
                elif headIndex + 1 > queueMaxSize then 0
                else headIndex + 1
            // printfn "tail %i" tailIndex
            // printfn "head %i" headIndex
            // printfn "new head %i" newHeadIndex
            headStream.Seek(0 |> int64, SeekOrigin.Begin)
            use headWriter = new StreamWriter(headStream)
            headWriter.WriteLine(newHeadIndex)
            return message
    with error ->
        printfn "%s" error.Message
        do! Async.Sleep 100
        return! dequeueAsync queueName }

// let dequeueAsync queueName = async {
//     return "line" }

let hasMessagesAsync queueName = async {
    let queuePath = getQueuePath queueName
    let line = Util.IO.File.firstLine queuePath
    let hasContent = line <> ""
    return  hasContent }

let persist queueName destinationPath = async {
    let queuePath = getQueuePath queueName
    Util.IO.File.copy queuePath destinationPath }
