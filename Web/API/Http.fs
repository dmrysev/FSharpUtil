module Util.API.Web.Http

type Events = { HttpError: IEvent<ErrorDetails> }
and ErrorDetails = { Error: System.Net.Http.HttpRequestException; Attempt: int }

type Config = { 
    Headers:  seq<string * string> option
    RetryOnHttpError: bool
    MaxRetriesOnHttpError: int
    HttpErrorRetryTimeout: System.TimeSpan }
with static member Default = {
        Headers = None
        RetryOnHttpError = true
        MaxRetriesOnHttpError = 3
        HttpErrorRetryTimeout = System.TimeSpan.FromMinutes(1.0) }

type Proxy = {
    Ip: string
    Port: int }
