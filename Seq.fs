module Util.Seq

let isSubset superSet set = set |> Seq.except superSet |> Seq.isEmpty

let hasOverlap seqA seqB = 
    let resultSeqA = seqA |> Seq.except seqB
    (resultSeqA |> Seq.length) < (seqA |> Seq.length)

let replace oldItem newItem seq = 
    seq
    |> Seq.except [oldItem]
    |> Seq.append [newItem]

let hasItemAt index items =
    index >= 0 && index < (items |> Seq.length)

let removeItem item items =
    let index = items |> Seq.findIndex (fun x -> x = item)
    items |> Seq.removeAt index

let tryFindItemIndex item items =
    items |> Seq.tryFindIndex (fun x -> x = item)
