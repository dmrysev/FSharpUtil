module Util.Math

let decimalPart (n: float) = n - System.Math.Truncate(n)

let hasDecimals (n: float) =
    decimalPart n > 0.0