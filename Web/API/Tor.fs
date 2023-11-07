module Util.API.Web.Tor

type Config = {
    Enabled: bool
    Ip: string
    Port: int
    ControlPort: int }
with static member Default = {
        Enabled = false
        Ip = ""
        Port = 0
        ControlPort = 0 }