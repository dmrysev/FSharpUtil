module Util.Map

let append (m1: Map<'a,'b>) (m2: Map<'a,'b>) = Map.foldBack Map.add m2 m1
