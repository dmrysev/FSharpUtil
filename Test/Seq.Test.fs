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