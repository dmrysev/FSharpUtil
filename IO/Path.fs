module Util.IO.Path

let (/) path1 path2 = System.IO.Path.Combine(path1, path2)