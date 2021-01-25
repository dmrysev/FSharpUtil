module Util.Seq

let isSubset set superSet = set |> Seq.except superSet |> Seq.isEmpty