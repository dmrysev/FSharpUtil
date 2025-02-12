module Util.API.Web.Http

type Events = { HttpError: IEvent<ErrorDetails> }
and ErrorDetails = { Error: System.Exception; Attempt: int }

type Config = { 
    Headers:  seq<string * string> option
    Cookies:  seq<string * string> option
    RetryOnHttpError: bool
    MaxRetriesOnError: int
    ErrorRetryTimeout: System.TimeSpan }
with static member Default = {
        Headers = None
        Cookies = None
        RetryOnHttpError = true
        MaxRetriesOnError = 3
        ErrorRetryTimeout = System.TimeSpan.FromMinutes(1.0) }

type Proxy = {
    Ip: string
    Port: int }
