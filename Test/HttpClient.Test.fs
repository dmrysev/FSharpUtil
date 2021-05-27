module Util.HttpClient.Test

open NUnit.Framework
open FsUnit
open FSharp.Data

[<TestCase("Error Response status code does not indicate success: 502 (ConnectionRefused)", 502)>]
let ``Parse http request error message`` errorMessage expectedCode =
    parseHttpRequestErrorMessageStatusCode errorMessage |> should equal expectedCode