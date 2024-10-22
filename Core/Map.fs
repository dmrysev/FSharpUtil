module Util.Map

/// <summary>
/// Appends two maps. In case of overlapping keys, entries from the second map (m2) override those in the first map (m1).
/// </summary>
/// <param name="m1">The first map.</param>
/// <param name="m2">The second map whose entries will override those in m1 in case of key conflicts.</param>
/// <returns>A new map containing all entries from both maps, with m2's entries overriding m1's on overlapping keys.</returns>
let append (m1: Map<'a,'b>) (m2: Map<'a,'b>) = Map.foldBack Map.add m2 m1
