module Util.Json

open Newtonsoft.Json

let toJson message = JsonConvert.SerializeObject message
let fromJson<'a> json = JsonConvert.DeserializeObject<'a> json
