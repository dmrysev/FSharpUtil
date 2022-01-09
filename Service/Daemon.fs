module Util.Service.Daemon

let init (timeout: System.TimeSpan) func = async {
    while true do
        do! func()
        do! Util.Async.sleep timeout 
}
