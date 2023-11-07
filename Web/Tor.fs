module Util.Web.Tor

open MihaZupan

let initHttpClient(config: Util.API.Web.Tor.Config) = 
    let proxy = HttpToSocks5Proxy(config.Ip, config.Port)
    let handler = new System.Net.Http.HttpClientHandler(Proxy = proxy, UseCookies = true)
    new System.Net.Http.HttpClient(handler)

let resetIdentity (config: Util.API.Web.Tor.Config) =
    let controlPortClient = DotNetTor.ControlPort.Client(config.Ip, controlPort = config.ControlPort, password = "rookie")
    controlPortClient.ChangeCircuitAsync().Wait()
