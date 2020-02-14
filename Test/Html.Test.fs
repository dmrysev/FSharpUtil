module Util.Html.Test

open NUnit.Framework
open FsUnit
open FSharp.Data

[<Test>]
let ``Given HtmlDocument, calling findLinks, must return list of links and their names`` () =
    let content = "
        <html>\
        <body>\
            <a href=\"test_link_1\">Test text 1</a>\
            <a href=\"test_link_2\">Test text 2</a>\ 
        </body>\
        </html>"

    let html = HtmlDocument.Parse content
    findLinks html |> should equivalent [("Test text 1", "test_link_1"); ("Test text 2", "test_link_2")]

[<Test>]
let ``Given HtmlDocument, calling findImages, must return list of image links and their names`` () =
    let content = "
        <html>\
        <body>\
            <img src=\"test_link_1\" alt=\"Test text 1\"/>\
            <img src=\"test_link_2\" alt=\"Test text 2\"/>\
        </body>\
        </html>"

    let html = HtmlDocument.Parse content
    findImages html |> should equivalent [("Test text 1", "test_link_1"); ("Test text 2", "test_link_2")]