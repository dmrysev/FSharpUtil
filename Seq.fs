module Util.Seq

let isSubset superSet set = set |> Seq.except superSet |> Seq.isEmpty

let hasOverlap seqA seqB = 
    let resultSeqA = seqA |> Seq.except seqB
    (resultSeqA |> Seq.length) < (seqA |> Seq.length)

let replace oldItem newItem seq = 
    seq
    |> Seq.except [oldItem]
    |> Seq.append [newItem]