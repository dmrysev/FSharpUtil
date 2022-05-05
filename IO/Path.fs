module Util.IO.Path

let invalidCharacters = ['/'; '<'; '>'; ':'; '"'; '/'; '\\'; '|'; '?'; '*']

let exists (path: string) = 
    System.IO.Directory.Exists path || System.IO.File.Exists path

let isDirectory path =
    let attributes = System.IO.File.GetAttributes path
    attributes.HasFlag(System.IO.FileAttributes.Directory)

let directorySparator = System.IO.Path.DirectorySeparatorChar
let isAbsolute path = path |> Util.String.startsWith (string directorySparator)

let realPath (path: string) = Util.Process.execute $"realpath '{path}'"

type FileName (str: string) = 
    let path = 
        if str = "" then raise (System.ArgumentException "File name can't be empty")
        if str.Contains(string directorySparator) then raise (System.ArgumentException "File name can't contain directory separator")
        str
    member this.Value = path
    static member value (fileName: FileName) = fileName.Value

    member this.Extension = System.IO.Path.GetExtension this.Value
    member this.WithoutExtension = this.Value |> Util.String.remove this.Extension |> FileName
    member this.Remove (toRemove: string) = this.Value |> Util.String.remove toRemove |> FileName

    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? FileName as d -> this.Value = d.Value
        | _ -> false

module FileName =
    let setExtension (extension: string) (fileName: FileName) =
        let extension = extension |> Util.String.remove "."
        FileName (sprintf "%s.%s" fileName.Value extension)

    let remove (toRemove: string) (fileName: FileName) = fileName.Remove toRemove
    let withoutExtension (fileName: FileName) = fileName.WithoutExtension

type DirectoryName(str: string) =
    let path = 
        if str = "" then raise (System.ArgumentException "Directory name can't be empty")
        if str.Contains(string directorySparator) then raise (System.ArgumentException "Directory name can't contain path separator")
        str
    member this.Value = path
    static member value (dirName: DirectoryName) = dirName.Value
    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? DirectoryName as d -> this.Value = d.Value
        | _ -> false

type FilePath(str: string) =
    let path = 
        if str = "" then raise (System.ArgumentException "Path can't be empty")
        str
    member this.Value = path
    static member value (dirPath: FilePath) = dirPath.Value
    static member exists (filePath: FilePath) = System.IO.File.Exists filePath.Value
    static member IsAbsolute (filePath: FilePath) = filePath.Value |> isAbsolute
    member this.isAbsolute = this |> FilePath.IsAbsolute
    member this.FileName = path |> System.IO.Path.GetFileName |> FileName
    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? FilePath as d -> this.Value = d.Value
        | _ -> false

type DirectoryPath(str: string) =
    let path = 
        if str = "" then raise (System.ArgumentException "Path can't be empty")
        str

    member this.Value = path
    static member value (dirPath: DirectoryPath) = dirPath.Value

    static member IsAbsolute (dirPath: DirectoryPath) =  dirPath.Value |> isAbsolute
    member this.isAbsolute = this |> DirectoryPath.IsAbsolute

    static member (/) (path1: DirectoryPath, path2: DirectoryPath) = 
        if path2.isAbsolute then raise (System.ArgumentException "Can't join with absolute path")
        let combined = System.IO.Path.Combine(path1.Value, path2.Value)
        DirectoryPath combined

    static member (/) (path1: DirectoryPath, path2: DirectoryName) = 
        let combined = System.IO.Path.Combine(path1.Value, path2.Value)
        DirectoryPath combined

    static member (/) (path1: DirectoryPath, path2: FilePath) = 
        if path2.isAbsolute then raise (System.ArgumentException "Can't join with absolute path")
        let combined = System.IO.Path.Combine(path1.Value, path2.Value)
        FilePath combined

    static member (/) (path1: DirectoryPath, path2: FileName) = 
        let combined = System.IO.Path.Combine(path1.Value, path2.Value)
        FilePath combined

    member this.DirectoryName = System.IO.Path.GetFileName path |> DirectoryName

    override this.GetHashCode () = this.Value.GetHashCode()
    override this.Equals other =
        match other with
        | :? DirectoryPath as d -> this.Value = d.Value
        | _ -> false

type FilePath with
    static member directoryPath (filePath: FilePath) =
        let dirPath = System.IO.Path.GetDirectoryName filePath.Value
        DirectoryPath dirPath
    member this.DirectoryPath = FilePath.directoryPath this
    member this.WithNewFileNamePreserveExtension newName =
        let extension = this.FileName.Extension
        let newFileName = FileName newName |> FileName.setExtension extension
        this.DirectoryPath/newFileName
    static member relativeTo (dirPath: DirectoryPath) (filePath: FilePath) = 
        let toRemove = $"{dirPath.Value}/"
        filePath.Value 
        |> Util.String.remove toRemove
        |> FilePath
    static member hasDirectoryPath (filePath: FilePath) =
        let dirPath = System.IO.Path.GetDirectoryName filePath.Value
        dirPath <> ""

module DirectoryPath =
    let directoryName (dirPath: DirectoryPath) = dirPath.DirectoryName

    let relativeTo (dirPath1: DirectoryPath) (dirPath2: DirectoryPath) = 
        let toRemove = $"{dirPath1.Value}/"
        dirPath2.Value 
        |> Util.String.remove toRemove
        |> DirectoryPath

