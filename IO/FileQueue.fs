module Util.IO.FileQueue

let initLockFilePath filePath =
    filePath + ".lock"

let rec waitForFileUnlockAsync filePath = async {
    let lockFilePath = initLockFilePath filePath
    if Util.IO.Path.exists lockFilePath then 
        do! Async.Sleep 5
        do! waitForFileUnlockAsync filePath }

let lockFile filePath = 
    let lockFilePath = initLockFilePath filePath
    Util.IO.File.create lockFilePath

let unlockFile filePath = 
    let lockFilePath = initLockFilePath filePath
    Util.IO.File.delete lockFilePath

let enqueueAsync filePath line = async {
    do! waitForFileUnlockAsync filePath
    lockFile filePath
    Util.IO.File.appendLine filePath line
    unlockFile filePath }

let dequeueAsync filePath = async {
    do! waitForFileUnlockAsync filePath
    lockFile filePath
    let line = Util.IO.File.popFirstLine filePath
    unlockFile filePath
    return line }

let copyFileAsync filePath destinationPath = async {
    do! waitForFileUnlockAsync filePath
    lockFile filePath
    Util.IO.File.copy filePath destinationPath
    unlockFile filePath }
