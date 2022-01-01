module Util.Async

type ScopedCancellationTokenSource() =
    let cancellationTokenSource = new System.Threading.CancellationTokenSource()
    interface System.IDisposable with
        member this.Dispose() = 
            cancellationTokenSource.Cancel()
    member this.Token = cancellationTokenSource.Token

let sleep (timespan: System.TimeSpan) =
    let timeoutMilliseconds = timespan.TotalMilliseconds |> int
    Async.Sleep timeoutMilliseconds

