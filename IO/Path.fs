module Util.IO.Path

let invalidCharacters = ['/'; '<'; '>'; ':'; '"'; '/'; '\\'; '|'; '?'; '*']
let directorySparator = System.IO.Path.DirectorySeparatorChar

type FileName (value: string) = 
    do
        if value = "" then raise (System.ArgumentException "File name can't be empty")
        if value.Contains(string directorySparator) then raise (System.ArgumentException "File name can't contain directory separator")
    member this.Value = value
    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? FileName as d -> this.Value = d.Value
        | _ -> false

type DirectoryName(value: string) =
    do
        if value = "" then raise (System.ArgumentException "Directory name can't be empty")
        if value.Contains(string directorySparator) then raise (System.ArgumentException "Directory name can't contain path separator")
    member this.Value = value
    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? DirectoryName as d -> this.Value = d.Value
        | _ -> false

type FilePath(value: string) =
    do 
        if value = "" then raise (System.ArgumentException "Path can't be empty")
    member this.Value = value
    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? FilePath as d -> this.Value = d.Value
        | _ -> false

type DirectoryPath(value: string) =
    do
        if value = "" then raise (System.ArgumentException "Path can't be empty")
    member this.Value = value
    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? DirectoryPath as d -> this.Value = d.Value
        | _ -> false

type Url (value: string) =
    do
        if value = "" then raise (System.ArgumentException("Url can't be empty"))
    member this.Value = value |> Util.String.removeLastCharacterIfEquals "/"
    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? Url as u -> this.Value = u.Value
        | _ -> false

let exists (path: string) = System.IO.Directory.Exists path || System.IO.File.Exists path
let isDirectory (path: string) =
    let attributes = System.IO.File.GetAttributes path
    attributes.HasFlag(System.IO.FileAttributes.Directory)
let isAbsolute path = path |> Util.String.startsWith (string directorySparator)
let realPath (path: string) = Util.Process.execute $"realpath '{path}'"
let isSymbolicLink (path: string) =
    let pathInfo = System.IO.FileInfo path
    pathInfo.Attributes.HasFlag(System.IO.FileAttributes.ReparsePoint)  

module FileName =
    let value (fileName: FileName) = fileName.Value
    let extension(fileName: FileName) = System.IO.Path.GetExtension fileName.Value
    let setExtension (extension: string) (fileName: FileName) =
        let extension = extension |> Util.String.remove "."
        FileName (sprintf "%s.%s" fileName.Value extension)
    let withoutExtension(fileName: FileName) = 
        let extension = fileName |> extension
        fileName.Value |> Util.String.remove extension |> FileName
    let remove (toRemove: string) (fileName: FileName) = fileName.Value |> Util.String.remove toRemove |> FileName

module DirectoryName =
    let value (dirName: DirectoryName) = dirName.Value

module FilePath =
    let value (dirPath: FilePath) = dirPath.Value
    let exists (filePath: FilePath) = System.IO.File.Exists filePath.Value
    let isAbsolute (filePath: FilePath) = filePath.Value |> isAbsolute
    let fileName (filePath: FilePath) = filePath.Value |> System.IO.Path.GetFileName |> FileName
    let fileNameWithoutExtension (filePath: FilePath) = filePath |> fileName |> FileName.withoutExtension
    let fileExtension (filePath: FilePath) = filePath |> fileName |> FileName.extension
    let realPath (filePath: FilePath) = realPath filePath.Value |> FilePath
    let directoryPath (filePath: FilePath) =
        let dirPath = System.IO.Path.GetDirectoryName filePath.Value
        DirectoryPath dirPath
    let relativeTo (dirPath: DirectoryPath) (filePath: FilePath) = 
        let toRemove = $"{dirPath.Value}/"
        filePath.Value 
        |> Util.String.remove toRemove
        |> FilePath
    let hasDirectoryPath (filePath: FilePath) =
        let dirPath = System.IO.Path.GetDirectoryName filePath.Value
        dirPath <> ""

module DirectoryPath =
    let value (dirPath: DirectoryPath) = dirPath.Value
    let isAbsolute (dirPath: DirectoryPath) =  dirPath.Value |> isAbsolute
    let relativeTo (dirPath1: DirectoryPath) (dirPath2: DirectoryPath) = 
        let toRemove = $"{dirPath1.Value}/"
        dirPath2.Value 
        |> Util.String.remove toRemove
        |> DirectoryPath
    let directoryName(dirPath: DirectoryPath) = System.IO.Path.GetFileName dirPath.Value |> DirectoryName
    let parent(dirPath: DirectoryPath) = 
        let parent = System.IO.Directory.GetParent dirPath.Value
        parent.ToString() |> DirectoryPath
    let realPath (dirPath: DirectoryPath) = realPath dirPath.Value |> DirectoryPath

type DirectoryPath with
    static member (/) (path1: DirectoryPath, path2: DirectoryPath) = 
        if path2 |> DirectoryPath.isAbsolute then raise (System.ArgumentException "Can't join with absolute path")
        let combined = System.IO.Path.Combine(path1.Value, path2.Value)
        DirectoryPath combined
    static member (/) (path1: DirectoryPath, path2: DirectoryName) = 
        let combined = System.IO.Path.Combine(path1.Value, path2.Value)
        DirectoryPath combined
    static member (/) (path1: DirectoryPath, path2: FilePath) = 
        if path2 |> FilePath.isAbsolute then raise (System.ArgumentException "Can't join with absolute path")
        let combined = System.IO.Path.Combine(path1.Value, path2.Value)
        FilePath combined
    static member (/) (path1: DirectoryPath, path2: FileName) = 
        let combined = System.IO.Path.Combine(path1.Value, path2.Value)
        FilePath combined
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
    let fileName (url: Url) = url.Value |> System.IO.Path.GetFileName |> FileName
    let isMatch (regexPattern: string) (url: Url) = url |> isMath regexPattern
    let extension (url: Url) = System.IO.Path.GetExtension url.Value
    let remove (toRemove: string) (url: Url) =
        url.Value
        |> Util.String.remove toRemove
        |> Url