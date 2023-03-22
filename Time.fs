module Util.Time

open System

type Range = { 
    Start: System.TimeSpan
    End: System.TimeSpan }

let splitRangeByInterval (range: Range) (interval: TimeSpan) =
    range.Start
    |> Seq.unfold (fun state ->
        if state >= range.End then None
        else
            let time = state + interval
            Some(state, time))
