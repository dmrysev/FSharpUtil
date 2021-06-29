module Util.Retry
open System.Threading

let withRetry (delay: System.TimeSpan) (maximumAttempts: int) func =
    let rec tryRun(attempt: int) =
        try func()
        with error ->
            if attempt = maximumAttempts then raise error
            printfn "Error %s. Attempt %i. Will try again in %A" error.Message attempt delay
            Thread.Sleep delay
            tryRun(attempt + 1)
    tryRun(1)