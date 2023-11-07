module Util.Memoization

let memoize f =
    let cache = ref Map.empty
    fun x ->
        match cache.Value.TryFind(x) with
        | Some res -> res
        | None ->
            let res = f x
            cache.Value <- cache.Value.Add(x, res)
            res
