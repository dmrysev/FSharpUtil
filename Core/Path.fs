module Util.Path

open Newtonsoft.Json

let invalidCharacters = ['/'; '<'; '>'; ':'; '"'; '/'; '\\'; '|'; '?'; '*']
let directorySeparatorChar = '/'

let hasDirectorySparatorChar path = path |> Seq.contains directorySeparatorChar

let normalizePath path = 
    path
    |> Util.String.replace "\\" "/"
    |> Util.String.replace "//" "/"

type FileName (value: string) = 
    do
        if value = "" then raise (System.ArgumentException "File name can't be empty")
        if hasDirectorySparatorChar value then raise (System.ArgumentException "File name can't contain directory separator")
    member this.Value = value
    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? FileName as d -> this.Value = d.Value
        | _ -> false
    override this.ToString() = value
    static member None = FileName "none"

type DirectoryName(value: string) =
    do
        if value = "" then raise (System.ArgumentException "Directory name can't be empty")
        if hasDirectorySparatorChar value then raise (System.ArgumentException "Directory name can't contain path separator")
    member this.Value = value
    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? DirectoryName as d -> this.Value = d.Value
        | _ -> false
    override this.ToString() = value
    static member None = DirectoryName "none"

type FilePath(value: string) =
    do 
        if value = "" then raise (System.ArgumentException "Path can't be empty")
    member this.Value = value |> normalizePath
    override this.GetHashCode() = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? FilePath as d -> this.Value = d.Value
        | _ -> false
    override this.ToString() = value
    static member None = FilePath "none"

type DirectoryPath(value: string) =
    do
        if value = "" then raise (System.ArgumentException "Path can't be empty")
    member this.Value = value |> normalizePath
    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? DirectoryPath as d -> this.Value = d.Value
        | _ -> false
    override this.ToString() = value
    static member None = DirectoryPath "none"

type Url (value: string) =
    do
        if value = "" then raise (System.ArgumentException("Url can't be empty"))
    member this.Value = value |> Util.String.removeLastCharacterIfEquals "/"
    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? Url as u -> this.Value = u.Value
        | _ -> false
    override this.ToString() = value
    static member None = Url "none"

type Path = File of FilePath | Directory of DirectoryPath

let value (path: Path) =
    match path with
    | Directory dirPath -> dirPath.Value
    | File filePath -> filePath.Value
let isAbsolute path = 
    let firstChar = path |> Seq.head 
    firstChar = directorySeparatorChar

let isInside (dirPath: DirectoryPath) (path: Path) =
    match path with
    | File path -> path.Value |> Util.String.startsWith dirPath.Value
    | Directory path -> path.Value |> Util.String.startsWith dirPath.Value

module FileName =
    let value (fileName: FileName) = fileName.Value
    let extension(fileName: FileName) = 
        fileName.Value
        |> Util.String.split "."
        |> Seq.last
    let setExtension (extension: string) (fileName: FileName) =
        let extension = extension |> Util.String.removeFirstCharacterIfEquals "."
        FileName (sprintf "%s.%s" fileName.Value extension)
    let withoutExtension(fileName: FileName) = 
        let extension = fileName |> extension
        fileName.Value 
        |> Util.String.remove extension 
        |> Util.String.removeLastCharacterIfEquals "."
        |> FileName
    let remove (toRemove: string) (fileName: FileName) = fileName.Value |> Util.String.remove toRemove |> FileName
    let hasExtension (extension: string) (fileName: FileName) = fileName.Value |> Util.String.endsWith extension
    let parseJsonObj (json: obj) = json |> string |> JsonConvert.DeserializeObject<FileName>
    let hasVideoExtension (fileName: FileName) = fileName.Value |> Util.StringMatch.isVideoFile
    let hasImageExtension (fileName: FileName) = fileName.Value |> Util.StringMatch.isImageFile

module DirectoryName =
    let value (dirName: DirectoryName) = dirName.Value

module FilePath =
    let value (dirPath: FilePath) = dirPath.Value
    let isAbsolute (filePath: FilePath) = filePath.Value |> isAbsolute
    let fileName (filePath: FilePath) = 
        let lastSeparatorIndex =
            filePath.Value 
            |> Seq.findIndexBack (fun c -> c = directorySeparatorChar)
        filePath.Value 
        |> Util.String.tail (lastSeparatorIndex + 1)
        |> FileName
    let fileNameWithoutExtension (filePath: FilePath) = filePath |> fileName |> FileName.withoutExtension
    let fileExtension (filePath: FilePath) = filePath |> fileName |> FileName.extension
    let directoryPath (filePath: FilePath) =
        let lastSeparatorIndex =
            filePath.Value 
            |> Seq.findIndexBack (fun c -> c = directorySeparatorChar)
        filePath.Value
        |> Util.String.head lastSeparatorIndex
        |> DirectoryPath
    let relativeTo (dirPath: DirectoryPath) (filePath: FilePath) = 
        let toRemove = $"{dirPath.Value}/"
        filePath.Value 
        |> Util.String.remove toRemove
        |> FilePath
    let toRelativePath (filePath: FilePath) =
        if filePath |> isAbsolute then
            filePath.Value
            |> Util.String.removeFirstCharacterIfEquals "/"
            |> FilePath
        else filePath
    let parseJsonObj (json: obj) = json |> string |> JsonConvert.DeserializeObject<FilePath>
    let hasVideoExtension (filePath: FilePath) = filePath |> fileName |> FileName.hasVideoExtension
    let hasImageExtension (filePath: FilePath) = filePath |> fileName |> FileName.hasImageExtension
    let hasExtension (extension: string) (filePath: FilePath) = 
        filePath
        |> fileName
        |> FileName.hasExtension extension

module DirectoryPath =
    let value (dirPath: DirectoryPath) = dirPath.Value
    let isAbsolute (dirPath: DirectoryPath) =  dirPath.Value |> isAbsolute
    let relativeTo (dirPath1: DirectoryPath) (dirPath2: DirectoryPath) = 
        let toRemove = $"{dirPath1.Value}/"
        dirPath2.Value 
        |> Util.String.remove toRemove
        |> DirectoryPath
    let directoryName(dirPath: DirectoryPath) = 
        dirPath.Value
        |> Util.String.split "/"
        |> Seq.last
        |> DirectoryName
    let parent(dirPath: DirectoryPath) = 
        let dirName = directoryName dirPath
        let parentString =
            dirPath.Value
            |> Util.String.remove dirName.Value
            |> Util.String.removeLastCharacterIfEquals "/"
        if parentString = "" then raise (System.ArgumentException "Path has no parent")
        else DirectoryPath parentString
    let toRelativePath(dirPath: DirectoryPath) = 
        if dirPath |> isAbsolute then
            dirPath.Value
            |> Util.String.removeFirstCharacterIfEquals "/"
            |> DirectoryPath
        else dirPath
    let parseJsonObj (json: obj) = json |> string |> JsonConvert.DeserializeObject<DirectoryPath>

type DirectoryPath with
    static member (/) (path1: DirectoryPath, path2: DirectoryPath) = 
        if path2 |> DirectoryPath.isAbsolute then raise (System.ArgumentException "Can't join with absolute path")
        DirectoryPath $"{path1.Value}{directorySeparatorChar}{path2.Value}"
    static member (/) (path1: DirectoryPath, path2: DirectoryName) = 
        DirectoryPath $"{path1.Value}{directorySeparatorChar}{path2.Value}"
    static member (/) (path1: DirectoryPath, path2: FilePath) = 
        if path2 |> FilePath.isAbsolute then raise (System.ArgumentException "Can't join with absolute path")
        FilePath $"{path1.Value}{directorySeparatorChar}{path2.Value}"
    static member (/) (path1: DirectoryPath, path2: FileName) = 
        FilePath $"{path1.Value}{directorySeparatorChar}{path2.Value}"
    static member setHeadDirectoryName (newName: string) (dirPath: DirectoryPath) =
        let parentDirPath = dirPath |> DirectoryPath.parent
        parentDirPath/DirectoryName newName

type FilePath with
    static member withNewFileNamePreserveExtension (newName: string) (filePath: FilePath) =
        let extension = filePath |> FilePath.fileName |> FileName.extension
        let newFileName = FileName newName |> FileName.setExtension extension
        let dirPath = filePath |> FilePath.directoryPath
        dirPath/newFileName

type Url with
    static member (/) (urlPath1: Url, urlPath2: Url) = 
        let combined = sprintf "%s/%s" urlPath1.Value urlPath2.Value
        Url combined
    static member (/) (urlPath1: Url, urlPath2: string) = 
        let combined = sprintf "%s/%s" urlPath1.Value urlPath2
        Url combined
    static member (/) (urlPath1: Url, urlPath2: int) = 
        let combined = sprintf "%s/%i" urlPath1.Value urlPath2
        Url combined
    static member (+) (urlPath: Url, value: string) = urlPath.Value + value |> Url
    static member (+) (urlPath: Url, value: int) = sprintf "%s%i" urlPath.Value value |> Url

module Url =
    let value (url: Url) = url.Value
    let fix (url: string) = url |> Util.String.removeLastCharacterIfEquals "/"
    let domainName(url: Url) = System.Uri(url.Value).Host
    let isMath (regexPattern: string) (url: Url) = url.Value |> Util.Regex.isMatch regexPattern
    let isDomainMatch (otherUrl: Url) (url: Url) = (url |> domainName) = (otherUrl |> domainName)
    let fileName (url: Url) = 
        let lastSeparatorIndex =
            url.Value 
            |> Seq.findIndexBack (fun c -> c = directorySeparatorChar)
        url.Value 
        |> Util.String.tail (lastSeparatorIndex + 1)
        |> FileName
    let isMatch (regexPattern: string) (url: Url) = url |> isMath regexPattern
    let extension (url: Url) = 
        url
        |> fileName
        |> FileName.extension
    let remove (toRemove: string) (url: Url) =
        url.Value
        |> Util.String.remove toRemove
        |> Url
    let parseJsonObj (json: obj) = json |> string |> JsonConvert.DeserializeObject<Url>
