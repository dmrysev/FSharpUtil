module Util.API.Web

type Events = { HttpError: IEvent<HttpErrorDetails> }
and HttpErrorDetails = { Error: System.Net.Http.HttpRequestException; Attempt: int }

type HttpConfig = { 
    Headers:  seq<string * string> option
    RetryOnHttpError: bool
    MaxRetriesOnHttpError: int
    HttpErrorRetryTimeout: System.TimeSpan }
with static member Default = {
        HttpConfig.Headers = None
        RetryOnHttpError = true
        MaxRetriesOnHttpError = 3
        HttpErrorRetryTimeout = System.TimeSpan.FromMinutes(1.0) }

type TorConfig = {
    Enabled: bool
    Ip: string
    Port: int
    ControlPort: int }
with static member Default = {
        Enabled = false
        Ip = ""
        Port = 0
        ControlPort = 0 }
