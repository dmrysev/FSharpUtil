module Util.Test.Map

open Util.Map
open NUnit.Framework
open FsUnit

[<Test>]
let ``Appending two empty maps should result in an empty map`` () =
    // ARRANGE
    let m1: Map<int, string> = Map.empty
    let m2: Map<int, string> = Map.empty

    // ACT
    let result = append m1 m2

    // ASSERT
    result |> should equal Map.empty

[<Test>]
let ``Appending an empty map and a non-empty map should return the non-empty map`` () =
    // ARRANGE
    let m1: Map<int, string> = Map.empty
    let m2 = Map.ofList [ (1, "one"); (2, "two") ]

    // ACT
    let result = append m1 m2

    // ASSERT
    result |> should equal m2

[<Test>]
let ``Appending a non-empty map and an empty map should return the non-empty map`` () =
    // ARRANGE
    let m1 = Map.ofList [ (1, "one"); (2, "two") ]
    let m2: Map<int, string> = Map.empty

    // ACT
    let result = append m1 m2

    // ASSERT
    result |> should equal m1

[<Test>]
let ``Appending two maps with non-overlapping keys should combine all entries`` () =
    // ARRANGE
    let m1 = Map.ofList [ (1, "one"); (2, "two") ]
    let m2 = Map.ofList [ (3, "three"); (4, "four") ]
    let expected = Map.ofList [ (1, "one"); (2, "two"); (3, "three"); (4, "four") ]

    // ACT
    let result = append m1 m2

    // ASSERT
    result |> should equal expected

[<Test>]
let ``Appending two maps with overlapping keys should prefer values from the first map`` () =
    // ARRANGE
    let m1 = Map.ofList [ (1, "ONE"); (2, "TWO") ]
    let m2 = Map.ofList [ (1, "one"); (3, "three") ]
    let expected = Map.ofList [ (1, "one"); (2, "TWO"); (3, "three") ]

    // ACT
    let result = append m1 m2

    // ASSERT
    result |> should equal expected

[<Test>]
let ``Appending maps with different key and value types should work correctly`` () =
    // ARRANGE
    let m1 = Map.ofList [ ("a", 1); ("b", 2) ]
    let m2 = Map.ofList [ ("b", 3); ("c", 4) ]
    let expected = Map.ofList [ ("a", 1); ("b", 3); ("c", 4) ]

    // ACT
    let result = append m1 m2

    // ASSERT
    result |> should equal expected