module Util.String.Test

open NUnit.Framework
open FsUnit

[<Test>]
let ``Given string and character, calling strip, must remove all characters from that string``() =
    "aaa" |> Util.String.strip "a" |> should equal ""
    "fafaaf" |> Util.String.strip "a" |> should equal "fff"
    "fafaAf" |> Util.String.strip "a" |> should equal "fff"
    "fafAAf" |> Util.String.strip "a" |> should equal "fff"
    "fafAAf" |> Util.String.strip "A" |> should equal "fff"
    "fafbfaafbbfabf" |> Util.String.strip "ab" |> should equal "ffffff"

[<Test>]
let ``Extract integer`` () =
    "abc123def" |> extractInt |> should equal 123
    "123abc" |> extractInt |> should equal 123
    "abc123" |> extractInt |> should equal 123
    "a1b2c3" |> extractInt |> should equal 1
    "0" |> extractInt |> should equal 0
    "abc123def456ghi789" |> extractInt |> should equal 123
    "-123abc" |> extractInt |> should equal 123
    (fun () -> extractInt "abcdef" |> ignore) |> should throw typeof<System.FormatException>
    (fun () -> extractInt "" |> ignore) |> should throw typeof<System.FormatException>

[<Test>]
let ``Replace sub-strings`` () =
    // Replace substring in the middle of the string
    "Hello world!" |> replace "world" "there" |> should equal "Hello there!"

    // Replace substring at the start of the string
    "Hello world!" |> replace "Hello" "Hi" |> should equal "Hi world!"

    // Replace substring at the end of the string
    "Hello world!" |> replace "world!" "there!" |> should equal "Hello there!"

    // Replace multiple occurrences of the substring
    "banana" |> replace "na" "nya" |> should equal "banyanya"

    // Replace with an empty string (effectively removing the substring)
    "Please remove this word." |> replace "remove" "" |> should equal "Please  this word."

    // Replace when the oldValue is not present (should return the original string)
    "This string has no match." |> replace "absent" "present" |> should equal "This string has no match."

    // Replace with the same oldValue and newValue (should return the original string)
    "This word will remain unchanged." |> replace "unchanged" "unchanged" |> should equal "This word will remain unchanged."

    // Replace empty oldValue (should throw an ArgumentException)
    (fun () -> "Test string." |> replace "" "new" |> ignore) |> should throw typeof<System.ArgumentException>

    // Replace when newValue is empty
    "Please delete this word." |> replace "delete" "" |> should equal "Please  this word."

    // Replace with case sensitivity (Replace is case-sensitive)
    "Hello world!" |> replace "World" "there" |> should equal "Hello world!" // "World" with uppercase W not present

    // Replace with different cases
    "Hello WORLD!" |> replace "WORLD" "there" |> should equal "Hello there!"

    // Replace a single character
    "banana" |> replace "a" "o" |> should equal "bonono"

    // Replace with special characters
    "Hello world!" |> replace " " "_" |> should equal "Hello_world!"

    // Replace when oldValue and newValue are overlapping substrings
    "banana" |> replace "ana" "anana" |> should equal "bananana"

    // Replace in an empty string (should return an empty string)
    "" |> replace "any" "replacement" |> should equal ""