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
    Util.IO.File.create (tempDirPath/FileName "file1")

    // ACT & ASSERT
    tempDirPath/FileName "file1" |> Util.IO.File.exists |> should be True
    tempDirPath/FileName "file2" |> Util.IO.File.exists |> should be False

[<Test>]
let ``Check if directory exists``() =
    // ARRANGE
    Util.IO.Directory.create (tempDirPath/DirectoryName "dir1")

    // ACT & ASSERT
    tempDirPath/DirectoryName "dir1" |> Util.IO.Directory.exists |> should be True
    tempDirPath/DirectoryName "dir2" |> Util.IO.Directory.exists  |> should be False

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
let ``Setting file name extension, must accept string with and without dot char``() =
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
    DirectoryPath @"\some\path\to\dirName" |> DirectoryPath.directoryName |> should equal (DirectoryName "dirName")

[<Test>]
let ``Get file name``() =
    FilePath "/some/path/to/file.txt" |> FilePath.fileName |> should equal (FileName "file.txt")

[<Test>]
let ``Get file name without extension``() =
    FilePath "/some/path/to/file.txt" |> FilePath.fileNameWithoutExtension |> should equal (FileName "file")

[<Test>]
let ``Convert absolute path to relative``() =
    FilePath "/some/path/to/file.txt" |> FilePath.toRelativePath |> should equal (FilePath "some/path/to/file.txt")
    DirectoryPath "/some/dir/path" |> DirectoryPath.toRelativePath |> should equal (DirectoryPath "some/dir/path")

// [<Test>]
// let ``FileName.remove should correctly remove specified substring``() =
//     FileName "example_file.txt"
//     |> FileName.remove "file"
//     |> should equal (FileName "example_.txt")

//     FileName "testdocument.pdf"
//     |> FileName.remove "doc"
//     |> should equal (FileName "testument.pdf")

// [<Test>]
// let ``FileName.hasExtension should correctly identify extensions``() =
//     FileName "photo.jpg" |> FileName.hasExtension "jpg" |> should be True
//     FileName "archive.tar.gz" |> FileName.hasExtension "gz" |> should be True
//     FileName "document.pdf" |> FileName.hasExtension "doc" |> should be False

// [<Test>]
// let ``FileName.hasVideoExtension should identify video files``() =
//     FileName "movie.mp4" |> FileName.hasVideoExtension |> should be True
//     FileName "clip.avi" |> FileName.hasVideoExtension |> should be True
//     FileName "song.mp3" |> FileName.hasVideoExtension |> should be False

// [<Test>]
// let ``FileName.hasImageExtension should identify image files``() =
//     FileName "image.png" |> FileName.hasImageExtension |> should be True
//     FileName "graphic.svg" |> FileName.hasImageExtension |> should be True
//     FileName "document.pdf" |> FileName.hasImageExtension |> should be False

// [<Test>]
// let ``FilePath.fileExtension should return correct extension``() =
//     FilePath "/path/to/file.txt" |> FilePath.fileExtension |> should equal "txt"
//     FilePath "/path/to/archive.tar.gz" |> FilePath.fileExtension |> should equal "gz"

// [<Test>]
// let ``FilePath.directoryPath should extract correct directory path``() =
//     FilePath "/path/to/file.txt" |> FilePath.directoryPath |> should equal (DirectoryPath "/path/to")
//     FilePath "/file.txt" |> FilePath.directoryPath |> should equal (DirectoryPath "/")

// [<Test>]
// let ``FilePath.relativeTo should correctly compute relative path``() =
//     let dir = DirectoryPath "/some/path"
//     let file = FilePath "/some/path/to/file.txt"
//     FilePath.relativeTo dir file |> should equal (FilePath "to/file.txt")

// [<Test>]
// let ``FilePath.relativeTo should correctly compute relative path when file is directly inside the directory``() =
//     let dir = DirectoryPath "/a/b"
//     let file = FilePath "/a/b/file.txt"
//     FilePath.relativeTo dir file |> should equal (FilePath "file.txt")

// [<Test>]
// let ``FilePath.relativeTo should correctly compute relative path when file is nested multiple levels deep``() =
//     let dir = DirectoryPath "/a/b"
//     let file = FilePath "/a/b/c/d/file.txt"
//     FilePath.relativeTo dir file |> should equal (FilePath "c/d/file.txt")

// [<Test>]
// let ``FilePath.relativeTo should handle directory paths with trailing slash``() =
//     let dir = DirectoryPath "/a/b/"
//     let file = FilePath "/a/b/c/file.txt"
//     FilePath.relativeTo dir file |> should equal (FilePath "c/file.txt")

// [<Test>]
// let ``FilePath.relativeTo should return the original path when directory path is root``() =
//     let dir = DirectoryPath "/"
//     let file = FilePath "/a/b/file.txt"
//     FilePath.relativeTo dir file |> should equal (FilePath "a/b/file.txt")

// [<Test>]
// let ``FilePath.relativeTo should handle file paths that are identical to directory path (resulting in empty relative path)``() =
//     let dir = DirectoryPath "/a/b"
//     let file = FilePath "/a/b"
//     FilePath.relativeTo dir file |> should equal (FilePath "")

// [<Test>]
// let ``FilePath.relativeTo should handle relative directory and file paths``() =
//     let dir = DirectoryPath "a/b"
//     let file = FilePath "a/b/c/file.txt"
//     FilePath.relativeTo dir file |> should equal (FilePath "c/file.txt")

// [<Test>]
// let ``FilePath.relativeTo should handle mixed absolute and relative paths``() =
//     let dir = DirectoryPath "/a/b"
//     let file = FilePath "a/b/c/file.txt"
//     // Assuming relativeTo expects both paths to be absolute or both to be relative
//     // This might raise an exception or return the original path
//     (fun () -> FilePath.relativeTo dir file |> ignore) |> should throw typeof<System.ArgumentException>

// [<Test>]
// let ``FilePath.relativeTo should throw exception when file path does not start with directory path``() =
//     let dir = DirectoryPath "/a/b"
//     let file = FilePath "/a/c/file.txt"
//     (fun () -> FilePath.relativeTo dir file |> ignore) |> should throw typeof<System.ArgumentException>

// [<Test>]
// let ``FilePath.relativeTo should throw exception when directory path is not a prefix of file path but shares a common root``() =
//     let dir = DirectoryPath "/a/b"
//     let file = FilePath "/a/bc/file.txt"
//     (fun () -> FilePath.relativeTo dir file |> ignore) |> should throw typeof<System.ArgumentException>

// [<Test>]
// let ``FilePath.relativeTo should handle file paths with redundant slashes``() =
//     let dir = DirectoryPath "/a/b"
//     let file = FilePath "/a/b//c///file.txt"
//     FilePath.relativeTo dir file |> should equal (FilePath "c/file.txt")

// [<Test>]
// let ``FilePath.relativeTo should handle directory path with redundant slashes``() =
//     let dir = DirectoryPath "/a//b/"
//     let file = FilePath "/a/b/c/file.txt"
//     FilePath.relativeTo dir file |> should equal (FilePath "c/file.txt")

// [<Test>]
// let ``FilePath.relativeTo should handle both directory and file paths with redundant slashes``() =
//     let dir = DirectoryPath "/a//b/"
//     let file = FilePath "/a/b//c///d/file.txt"
//     FilePath.relativeTo dir file |> should equal (FilePath "c/d/file.txt")

// [<Test>]
// let ``FilePath.relativeTo should handle directory path being a substring of file path but not a proper prefix``() =
//     let dir = DirectoryPath "/a/b/c"
//     let file = FilePath "/a/b/cd/file.txt"
//     (fun () -> FilePath.relativeTo dir file |> ignore) |> should throw typeof<System.ArgumentException>

// [<Test>]
// let ``FilePath.relativeTo should handle case sensitivity based on OS``() =
//     // This test assumes case-sensitive file systems.
//     // On case-insensitive systems, adjust expectations accordingly.
//     let dir = DirectoryPath "/A/B"
//     let file = FilePath "/a/b/c/file.txt"
//     (fun () -> FilePath.relativeTo dir file |> ignore) |> should throw typeof<System.ArgumentException>

// [<Test>]
// let ``FilePath.relativeTo should handle Unicode characters in paths``() =
//     let dir = DirectoryPath "/a/б"
//     let file = FilePath "/a/б/в/файл.txt"
//     FilePath.relativeTo dir file |> should equal (FilePath "в/файл.txt")

// [<Test>]
// let ``FilePath.relativeTo should handle empty relative path gracefully``() =
//     let dir = DirectoryPath "/a/b"
//     let file = FilePath "/a/b"
//     FilePath.relativeTo dir file |> should equal (FilePath "")

// [<Test>]
// let ``FilePath.relativeTo should handle file path being exactly one level deeper than directory path``() =
//     let dir = DirectoryPath "/a/b"
//     let file = FilePath "/a/b/c"
//     FilePath.relativeTo dir file |> should equal (FilePath "c")

// [<Test>]
// let ``FilePath.relativeTo should handle file paths with dot segments``() =
//     let dir = DirectoryPath "/a/b"
//     let file = FilePath "/a/b/./c/../c/file.txt"
//     // Assuming normalizePath has been applied, which would resolve to "/a/b/c/file.txt"
//     FilePath.relativeTo dir file |> should equal (FilePath "c/file.txt")

// [<Test>]
// let ``FilePath.relativeTo should handle directory paths ending without slash``() =
//     let dir = DirectoryPath "/a/b"
//     let file = FilePath "/a/bc/file.txt"
//     (fun () -> FilePath.relativeTo dir file |> ignore) |> should throw typeof<System.ArgumentException>

// [<Test>]
// let ``FilePath.hasVideoExtension should identify video files``() =
//     FilePath "/videos/movie.mp4" |> FilePath.hasVideoExtension |> should be True
//     FilePath "/videos/song.mp3" |> FilePath.hasVideoExtension |> should be False

// [<Test>]
// let ``FilePath.hasImageExtension should identify image files``() =
//     FilePath "/images/photo.png" |> FilePath.hasImageExtension |> should be True
//     FilePath "/images/document.pdf" |> FilePath.hasImageExtension |> should be False

// [<Test>]
// let ``FilePath.hasExtension should correctly identify specified extensions``() =
    FilePath "/docs/report.docx" |> FilePath.hasExtension "docx" |> should be True
    FilePath "/docs/report.docx" |> FilePath.hasExtension "pdf" |> should be False

[<Test>]
let ``FilePath.withNewFileNamePreserveExtension should preserve extension correctly``() =
    let originalPath = FilePath "/path/to/file.txt"
    let newPath = FilePath.withNewFileNamePreserveExtension "newfile" originalPath
    newPath |> should equal (FilePath "/path/to/newfile.txt")

    let originalPath2 = FilePath "/path/to/archive.tar.gz"
    let newPath2 = FilePath.withNewFileNamePreserveExtension "backup" originalPath2
    newPath2 |> should equal (FilePath "/path/to/backup.gz")

[<Test>]
let ``DirectoryPath.parent should return parent directory``() =
    DirectoryPath "/a/b/c" |> DirectoryPath.parent |> should equal (DirectoryPath "/a/b")

[<Test>]
let ``Calling DirectoryPath.parent on a path without should throw exception``() =
    (fun () -> DirectoryPath "/a" |> DirectoryPath.parent |> ignore) |> should throw typeof<System.ArgumentException>
    (fun () -> DirectoryPath "a" |> DirectoryPath.parent |> ignore) |> should throw typeof<System.ArgumentException>

// [<Test>]
// let ``DirectoryPath.relativeTo should correctly compute relative path``() =
//     let dir1 = DirectoryPath "/a/b"
//     let dir2 = DirectoryPath "/a/b/c/d"
//     DirectoryPath.relativeTo dir1 dir2 |> should equal (DirectoryPath "c/d")

//     let dir3 = DirectoryPath "/a/b/c/d/"
//     DirectoryPath.relativeTo dir1 dir3 |> should equal (DirectoryPath "c/d/")

// [<Test>]
// let ``Url operator / should combine URLs correctly``() =
//     let baseUrl = Url "http://example.com"
//     let combinedUrl = baseUrl / Url "path/to/resource"
//     combinedUrl |> should equal (Url "http://example.com/path/to/resource")

//     let combinedUrl2 = baseUrl / "another/resource"
//     combinedUrl2 |> should equal (Url "http://example.com/another/resource")

//     let combinedUrl3 = baseUrl / 1234
//     combinedUrl3 |> should equal (Url "http://example.com/1234")

// [<Test>]
// let ``Url operator + should concatenate correctly``() =
//     let baseUrl = Url "http://example.com/api"
//     let concatenatedUrl = baseUrl + "/v1"
//     concatenatedUrl |> should equal (Url "http://example.com/api/v1")

//     let concatenatedUrl2 = baseUrl + 2024
//     concatenatedUrl2 |> should equal (Url "http://example.com/api2024")

// [<Test>]
// let ``Url.isMatch should correctly match regex patterns``() =
//     let url = Url "http://example.com/resource/123"
//     url |> Url.isMatch "^http://example\.com/resource/\d+$" |> should be True
//     url |> Url.isMatch "^https://example\.com/resource/\d+$" |> should be False

// [<Test>]
// let ``Url.extension should return correct file extension``() =
//     let url1 = Url "http://example.com/file.png"
//     url1 |> Url.extension |> should equal "png"

//     let url2 = Url "http://example.com/path/to/document.pdf"
//     url2 |> Url.extension |> should equal "pdf"

//     let url3 = Url "http://example.com/path/to/resource" // No extension
//     (try Url.extension url3 |> ignore; false with _ -> true) |> should be True

// [<Test>]
// let ``Url.remove should correctly remove specified substring``() =
//     let url = Url "http://example.com/path/to/resource"
//     let updatedUrl = Url.remove "/path" url
//     updatedUrl |> should equal (Url "http://example.com/to/resource")

// [<Test>]
// let ``Url.fileName should extract correct file name``() =
//     let url1 = Url "http://example.com/path/to/file.txt"
//     url1 |> Url.fileName |> should equal (FileName "file.txt")

//     let url2 = Url "http://example.com/path/to/"
//     (try Url.fileName url2 |> ignore; false with _ -> true) |> should be True // No file name

// [<Test>]
// let ``Url.isDomainMatch should correctly compare domains``() =
//     let url1 = Url "http://example.com/path"
//     let url2 = Url "https://example.com/another/path"
//     let url3 = Url "http://different.com/path"
//     Url.isDomainMatch url1 url2 |> should be True
//     Url.isDomainMatch url1 url3 |> should be False

// // [<Test>]
// // let ``Path.isAbsolute should correctly identify absolute paths``() =
// //     FilePath "/absolute/path/file.txt" |> isAbsolute |> should be True
// //     DirectoryPath "/absolute/path/" |> isAbsolute |> should be True
// //     FilePath "relative/path/file.txt" |> isAbsolute |> should be False
// //     DirectoryPath "relative/path/" |> isAbsolute |> should be False

// [<Test>]
// let ``Path.isInside should correctly determine path hierarchy``() =
//     let parentDir = DirectoryPath "/a/b"
//     let childFile = FilePath "/a/b/c/file.txt"
//     let childDir = DirectoryPath "/a/b/c/d"

//     isInside parentDir (File childFile) |> should be True
//     isInside parentDir (Directory childDir) |> should be True

//     let unrelatedFile = FilePath "/a/c/file.txt"
//     isInside parentDir (File unrelatedFile) |> should be False

// [<Test>]
// let ``Path.value should return correct string representation``() =
//     let filePath = FilePath "/path/to/file.txt"
//     let dirPath = DirectoryPath "/path/to/directory/"
//     let filePathValue = Path.File filePath |> value
//     let dirPathValue = Path.Directory dirPath |> value

//     filePathValue |> should equal "/path/to/file.txt"
//     dirPathValue |> should equal "/path/to/directory/"

// // Equality and HashCode Tests

// [<Test>]
// let ``FileName equality and hashcode should work correctly``() =
//     let fileName1 = FileName "test.txt"
//     let fileName2 = FileName "test.txt"
//     let fileName3 = FileName "different.txt"

//     fileName1.Equals(fileName2) |> should be True
//     fileName1.Equals(fileName3) |> should be False
//     (fileName1.GetHashCode() = fileName2.GetHashCode()) |> should be True
//     (fileName1.GetHashCode() = fileName3.GetHashCode()) |> should be False

// [<Test>]
// let ``DirectoryPath equality and hashcode should work correctly``() =
//     let dirPath1 = DirectoryPath "/a/b/c"
//     let dirPath2 = DirectoryPath "/a/b/c"
//     let dirPath3 = DirectoryPath "/a/b/d"

//     dirPath1.Equals(dirPath2) |> should be True
//     dirPath1.Equals(dirPath3) |> should be False
//     (dirPath1.GetHashCode() = dirPath2.GetHashCode()) |> should be True
//     (dirPath1.GetHashCode() = dirPath3.GetHashCode()) |> should be False

// [<Test>]
// let ``FilePath equality and hashcode should work correctly``() =
//     let filePath1 = FilePath "/path/to/file.txt"
//     let filePath2 = FilePath "/path/to/file.txt"
//     let filePath3 = FilePath "/path/to/another.txt"

//     filePath1.Equals(filePath2) |> should be True
//     filePath1.Equals(filePath3) |> should be False
//     (filePath1.GetHashCode() = filePath2.GetHashCode()) |> should be True
//     (filePath1.GetHashCode() = filePath3.GetHashCode()) |> should be False

// [<Test>]
// let ``Url equality and hashcode should work correctly``() =
//     let url1 = Url "http://example.com/path"
//     let url2 = Url "http://example.com/path"
//     let url3 = Url "http://example.com/other"

//     url1.Equals(url2) |> should be True
//     url1.Equals(url3) |> should be False
//     (url1.GetHashCode() = url2.GetHashCode()) |> should be True
//     (url1.GetHashCode() = url3.GetHashCode()) |> should be False

// // NormalizePath Function Test

// [<Test>]
// let ``normalizePath should correctly replace backslashes and remove duplicate slashes``() =
//     let path1 = "C:\\Folder\\SubFolder//file.txt"
//     let normalized1 = normalizePath path1
//     normalized1 |> should equal "C:/Folder/SubFolder/file.txt"

//     let path2 = "\\\\Server\\Share\\Folder\\"
//     let normalized2 = normalizePath path2
//     normalized2 |> should equal "//Server/Share/Folder/"

//     let path3 = "relative\\path\\to//resource"
//     let normalized3 = normalizePath path3
//     normalized3 |> should equal "relative/path/to/resource"
