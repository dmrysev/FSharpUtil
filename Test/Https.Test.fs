module Util.Test.Http

open Util.Http
open NUnit.Framework
open FsUnit

[<Test>]
let ``Get url domain name``() =
    (Url "http://testdomain.com").DomainName |> should equal "testdomain.com"
    (Url "http://testdomain.com/").DomainName |> should equal "testdomain.com"
    (Url "http://testdomain.com/A").DomainName |> should equal "testdomain.com"
    (Url "http://testdomain.com/A/B").DomainName |> should equal "testdomain.com"
    (Url "http://testdomain.com/file.txt").DomainName |> should equal "testdomain.com"
    (Url "http://testdomain.com/A/file.txt").DomainName |> should equal "testdomain.com"

    (Url "https://testdomain.com").DomainName |> should equal "testdomain.com"
    (Url "https://testdomain.com/").DomainName |> should equal "testdomain.com"
    (Url "https://testdomain.com/A").DomainName |> should equal "testdomain.com"
    (Url "https://testdomain.com/A/B").DomainName |> should equal "testdomain.com"
    (Url "https://testdomain.com/file.txt").DomainName |> should equal "testdomain.com"
    (Url "https://testdomain.com/A/file.txt").DomainName |> should equal "testdomain.com"

[<Test>]
let ``Fix url``() =
    "https://source.com/view/1234" |> Url.fix |> should equal "https://source.com/view/1234"
    "https://source.com/view/1234/" |> Url.fix |> should equal "https://source.com/view/1234"