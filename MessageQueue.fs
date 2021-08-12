module Util.MessageQueue

open Util.IO.Path

let uid = "cb045d48-ac03-4dc2-b5d2-565aa32e70af"

let getQueuePath queueName =
    Util.Environment.SpecialFolder.temporary/uid/queueName

let getLockFilePath queueName =
    let queuePath = getQueuePath queueName
    queuePath + ".lock"

let rec waitForQueueUnlockAsync queueName = async {
    let lockFilePath = getLockFilePath queueName
    if Util.IO.Path.exists lockFilePath then 
        do! Async.Sleep 5
        do! waitForQueueUnlockAsync queueName }

let lockQueue queueName = 
    let lockFilePath = getLockFilePath queueName
    Util.IO.File.create lockFilePath

let unlockQueue queueName = 
    let lockFilePath = getLockFilePath queueName
    Util.IO.File.delete lockFilePath

let enqueueAsync queueName message = async {
    do! waitForQueueUnlockAsync queueName
    lockQueue queueName
    let queuePath = getQueuePath queueName
    Util.IO.File.appendLine queuePath message
    unlockQueue queueName }

let dequeueAsync queueName = async {
    do! waitForQueueUnlockAsync queueName
    lockQueue queueName
    let queuePath = getQueuePath queueName
    let line = Util.IO.File.popFirstLine queuePath
    unlockQueue queueName
    return line }

let hasMessagesAsync queueName = async {
    do! waitForQueueUnlockAsync queueName
    lockQueue queueName
    let queuePath = getQueuePath queueName
    let line = Util.IO.File.firstLine queuePath
    let hasContent = line <> ""
    unlockQueue queueName
    return  hasContent }

let persist queueName destinationPath = async {
    do! waitForQueueUnlockAsync queueName
    lockQueue queueName
    let queuePath = getQueuePath queueName
    Util.IO.File.copy queuePath destinationPath
    unlockQueue queueName }
