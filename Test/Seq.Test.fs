module Util.Test.Seq

open Util.Seq
open NUnit.Framework
open FsUnit

[<Test>]
let ``Check if SeqA is subset of SeqB``() =
    [1] |> isSubset [1; 2; 3] |> should be True
    [1] |> isSubset [3; 2; 1] |> should be True
    [1] |> isSubset [3; 3; 1; 1; 2; 1] |> should be True
    [1; 3] |> isSubset [3; 3; 1; 1; 2; 1] |> should be True
    [1; 2] |> isSubset [1; 2; 3] |> should be True
    [1; 2; 3] |> isSubset [1; 2; 3] |> should be True
    [3] |> isSubset [1; 2] |> should be False
    [1] |> isSubset [2; 3] |> should be False
    [1; 2] |> isSubset [2; 3] |> should be False
    [1; 2] |> isSubset [3; 4] |> should be False
    [1; 2; 3; 4] |> isSubset [1; 2; 3] |> should be False

[<Test>]
let ``Check if SeqA has overlap with SeqB``() =
    [1] |> hasOverlap [1] |> should be True
    [1] |> hasOverlap [1; 2] |> should be True
    [2] |> hasOverlap [1; 2] |> should be True
    [1; 2] |> hasOverlap [1; 2] |> should be True
    [1; 2] |> hasOverlap [1; 2; 3] |> should be True
    [1; 2; 3] |> hasOverlap [1; 2; 3] |> should be True
    [1; 2] |> hasOverlap [3; 2; 1] |> should be True
    [1; 2; 3] |> hasOverlap [1] |> should be True
    [1; 2; 3] |> hasOverlap [1; 2] |> should be True
    [1; 2] |> hasOverlap [3; 3; 2; 1; 2; 2; 3] |> should be True
    [1] |> hasOverlap [2] |> should be False
    [1] |> hasOverlap [2; 3] |> should be False

[<Test>]
let ``Remove existing item from sequence``() =
    // ARRANGE
    let initialSeq = seq { 1; 2; 3 }

    // ACT
    let result = initialSeq |> Util.Seq.removeItem 2

    // ASSERT
    result |> should equal (seq { 1; 3 })

[<Test>]
let ``Move an item to the top of a seq``() =
    [ 1; 2; 3; 4; 5 ] |> Util.Seq.moveToTop 3 |> should equal [ 3; 1; 2; 4; 5 ]
    [ 3; 1; 2; 4; 5 ] |> Util.Seq.moveToTop 3 |> should equal [ 3; 1; 2; 4; 5 ]
    [ 1; 2; 4; 5 ] |> Util.Seq.moveToTop 3 |> should equal [ 3; 1; 2; 4; 5 ]
    [ 3 ] |> Util.Seq.moveToTop 3 |> should equal [ 3 ]
    [ 1 ] |> Util.Seq.moveToTop 3 |> should equal [ 3; 1 ]
    [] |> Util.Seq.moveToTop 3 |> should equal [ 3 ]
    [ 1; 3; 3; 4; 5 ] |> Util.Seq.moveToTop 3 |> should equal [ 3; 1; 4; 5 ]