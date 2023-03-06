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

let tryFindItem item items =
    match items |> Seq.tryFindIndex (fun x -> x = item) with
    | Some index -> items |> Seq.item index |> Some
    | None -> None

let tryFindItemBy predicate items =
    match items |> Seq.tryFindIndex predicate with
    | Some index -> items |> Seq.item index |> Some
    | None -> None

let appendItem item items = Seq.append items [item]
let prependItem item items = Seq.append [item] items

let tailN n items = items |> Seq.skip (Seq.length items - n)
