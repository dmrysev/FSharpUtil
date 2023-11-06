module Util.Test.IO.Path

open Util.Path
open NUnit.Framework
open FsUnit

let tempDirPath = Util.IO.Directory.generateTemporaryDirectory()

[<SetUp>]
let setUp() =
    ()

[<TearDown>]
let tearDown() =
    Util.IO.Directory.delete tempDirPath

[<Test>]
let ``Check if file exists``() =
    // ARRANGE
    Util.IO.File.create (tempDirPath/FileName "file_1")

    // ACT & ASSERT
    tempDirPath/FileName "file_1" |> Util.IO.File.exists |> should be True
    tempDirPath/FileName "file_2" |> Util.IO.File.exists |> should be False

[<Test>]
let ``Check if directory exists``() =
    // ARRANGE
    Util.IO.Directory.create (tempDirPath/DirectoryName "dir_1")

    // ACT & ASSERT
    tempDirPath/DirectoryName "dir_1" |> Util.IO.Directory.exists |> should be True
    tempDirPath/DirectoryName "dir_2" |> Util.IO.Directory.exists  |> should be False

[<Test>]
let ``If input string is invalid, initializing file name, must raise argument exception``() =
    (fun () -> FileName "" |> ignore) |> should throw typeof<System.ArgumentException>
    (fun () -> FileName "/file.txt" |> ignore) |> should throw typeof<System.ArgumentException>
    (fun () -> FileName "Dir/file.txt" |> ignore) |> should throw typeof<System.ArgumentException>

[<Test>]
let ``If input string is invalid, initializing directory name, must raise argument exception``() =
    (fun () -> DirectoryName "" |> ignore) |> should throw typeof<System.ArgumentException>
    (fun () -> DirectoryName "/A" |> ignore) |> should throw typeof<System.ArgumentException>
    (fun () -> DirectoryName "A/" |> ignore) |> should throw typeof<System.ArgumentException>
    (fun () -> DirectoryName "/A/" |> ignore) |> should throw typeof<System.ArgumentException>
    (fun () -> DirectoryName "/A/B" |> ignore) |> should throw typeof<System.ArgumentException>

[<Test>]
let ``Setting file name extens, must accept string with and without dot char``() =
    FileName "file" |> FileName.setExtension "txt" |> should equal (FileName "file.txt")
    FileName "file" |> FileName.setExtension ".txt" |> should equal (FileName "file.txt")

[<Test>]
let ``Initializing directory path, must fix string path``() =
    (DirectoryPath "/A/B").Value |> should equal "/A/B"
    (DirectoryPath "/A/B/").Value |> should equal "/A/B/"

[<Test>]
let ``Join together different path types``() =
    DirectoryPath "/A/B"/DirectoryPath "C" |> should equal (DirectoryPath "/A/B/C")
    DirectoryPath "/A/B/"/DirectoryPath "C" |> should equal (DirectoryPath "/A/B/C")
    DirectoryPath "/A/B"/DirectoryPath "C/D" |> should equal (DirectoryPath "/A/B/C/D")
    DirectoryPath "/A/B"/DirectoryName "C" |> should equal (DirectoryPath "/A/B/C")
    DirectoryPath "/A/B/"/DirectoryName "C" |> should equal (DirectoryPath "/A/B/C")
    DirectoryPath "/A/B"/FilePath "C/File.txt" |> should equal (FilePath "/A/B/C/File.txt")
    DirectoryPath "/A/B/"/FilePath "C/File.txt" |> should equal (FilePath "/A/B/C/File.txt")
    DirectoryPath "/A/B"/FileName "File.txt" |> should equal (FilePath "/A/B/File.txt")
    DirectoryPath "/A/B/"/FileName "File.txt" |> should equal (FilePath "/A/B/File.txt")

[<Test>]
let ``Trying to join with absolute directroy path, must throw argument exception``() =
    (fun () -> DirectoryPath "/A"/DirectoryPath "/C" |> ignore) |> should throw typeof<System.ArgumentException>

[<Test>]
let ``Trying to join with absolute file path, must throw argument exception``() =
    (fun () -> DirectoryPath "/A"/FilePath "/B/file.txt" |> ignore) |> should throw typeof<System.ArgumentException>

[<Test>]
let ``Get url domain name``() =
    (Url "http://testdomain.com") |> Url.domainName |> should equal "testdomain.com"
    (Url "http://testdomain.com/") |> Url.domainName |> should equal "testdomain.com"
    (Url "http://testdomain.com/A") |> Url.domainName |> should equal "testdomain.com"
    (Url "http://testdomain.com/A/B") |> Url.domainName |> should equal "testdomain.com"
    (Url "http://testdomain.com/file.txt") |> Url.domainName |> should equal "testdomain.com"
    (Url "http://testdomain.com/A/file.txt") |> Url.domainName |> should equal "testdomain.com"

    (Url "https://testdomain.com") |> Url.domainName |> should equal "testdomain.com"
    (Url "https://testdomain.com/") |> Url.domainName |> should equal "testdomain.com"
    (Url "https://testdomain.com/A") |> Url.domainName |> should equal "testdomain.com"
    (Url "https://testdomain.com/A/B") |> Url.domainName |> should equal "testdomain.com"
    (Url "https://testdomain.com/file.txt") |> Url.domainName |> should equal "testdomain.com"
    (Url "https://testdomain.com/A/file.txt") |> Url.domainName |> should equal "testdomain.com"

[<Test>]
let ``Fix url``() =
    "https://source.com/view/1234" |> Url.fix |> should equal "https://source.com/view/1234"
    "https://source.com/view/1234/" |> Url.fix |> should equal "https://source.com/view/1234"

[<Test>]
let ``Get directory name from directory path``() =
    DirectoryPath "/some/path/to/dirName" |> DirectoryPath.directoryName |> should equal (DirectoryName "dirName")
    DirectoryPath "\\some\\path\\to\\dirName" |> DirectoryPath.directoryName |> should equal (DirectoryName "dirName")

[<Test>]
let ``Get file name``() =
    FilePath "/some/path/to/file.txt" |> FilePath.fileName |> should equal (FileName "file.txt")

[<Test>]
let ``Get file name without extension``() =
    FilePath "/some/path/to/file.txt" |> FilePath.fileNameWithoutExtension |> should equal (FileName "file")
