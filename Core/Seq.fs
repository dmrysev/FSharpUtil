module Util.Seq

let isSubset superSet set = set |> Seq.except superSet |> Seq.isEmpty

let hasOverlap seqA seqB = 
    let resultSeqA = seqA |> Seq.except seqB
    (resultSeqA |> Seq.length) < (seqA |> Seq.length)

let replace oldItem newItem seq = 
    seq
    |> Seq.map (fun item ->
        if item = oldItem then newItem
        else item)

let hasItemAt index items =
    (items |> Seq.isEmpty |> not)
    && index >= 0 
    && index < (items |> Seq.length)

let removeItem item items =
    let index = items |> Seq.findIndex (fun x -> x = item)
    items |> Seq.removeAt index

let findItemIndex item items =
    items |> Seq.findIndex (fun x -> x = item)

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

let tryFindItemBack item items =
    match items |> Seq.tryFindIndexBack (fun x -> x = item) with
    | Some index -> items |> Seq.item index |> Some
    | None -> None

let tryFindItemBackBy predicate items =
    match items |> Seq.tryFindIndexBack predicate with
    | Some index -> items |> Seq.item index |> Some
    | None -> None

let appendItem item items = Seq.append items [item]
let prependItem item items = Seq.append [item] items

let tailN n items = items |> Seq.skip (Seq.length items - n)

let maxTail count items =
    if items |> Seq.length >= count
    then items |> tailN count
    else items

let lastIndex items = Seq.length items - 1

let shuffle seq =
    let array = seq |> Seq.toArray
    let random = System.Random()
    for i in 0 .. array.Length - 1 do
        let j = random.Next(i, array.Length)
        let pom = array.[i]
        array.[i] <- array.[j]
        array.[j] <- pom

    array |> Array.toSeq

let limitItems (startIndex: int) (maxResultCount: int) items = 
    let maxResultCount = 
        if (startIndex + maxResultCount) > (items |> Seq.length)
        then (items |> Seq.length) - startIndex
        else maxResultCount
    if not (items |> hasItemAt startIndex) then Seq.empty
    else
        items
        |> Seq.skip startIndex
        |> Seq.take maxResultCount

let moveToTop item items =
    let filteredSeq = items |> Seq.filter ((<>) item)
    seq {
        yield item
        yield! filteredSeq
    }