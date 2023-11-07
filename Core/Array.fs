module Util.Array

let rng = new System.Random()
let shuffle (arr : 'a array) =
    let array = Array.copy arr
    let n = array.Length
    for x in 1..n do
        let i = n-x
        let j = rng.Next(i+1)
        let tmp = array.[i]
        array.[i] <- array.[j]
        array.[j] <- tmp
    array