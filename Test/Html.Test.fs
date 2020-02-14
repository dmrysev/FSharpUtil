module Util.Html.Test

open NUnit.Framework
open FsUnit
open FSharp.Data
open Util.Html

[<Test>]
let ``Given html document, calling findLinks, must return list of links and their names`` () =
        let content = "<!DOCTYPE html>\
            <html lang=en\">\
            <body>\
                <a href=\"test_link_1\">Test text 1</a>\
                <a href=\"test_link_2\">Test text 2</a>\ 
            </body>\
            </html>"

        let html = HtmlDocument.Parse content
        findLinks html |> should equivalent [("Test text 1", "test_link_1"); ("Test text 2", "test_link_2")]