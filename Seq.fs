module Util.Seq

let isSubset superSet set = set |> Seq.except superSet |> Seq.isEmpty