module Util.Log

open System

let debugInfo details =
    #if DEBUG_LOG_INFO
    printfn "%A" details
    #endif
    ()

let debugError details =
    #if DEBUG_LOG_ERROR
    printfn "%A" details
    #endif
    ()

let info details = printfn $"{DateTime.Now} {details}"
let error details = printfn "%A" details
