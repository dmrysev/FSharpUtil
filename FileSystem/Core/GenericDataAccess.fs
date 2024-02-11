namespace Util.Core.FileSystem

open Util
open Util.Path

type GenericDataAccess<'Id, 'Data when 'Id: equality> = {
    List: unit -> 'Id seq
    Read: 'Id -> 'Data
    TryRead: 'Id -> 'Data option
    ReadAll: unit -> 'Data seq
    Write: 'Id -> 'Data -> unit
    WriteEvent: IEvent<'Id>
    Delete: 'Id -> unit
    EnsureDeleted: 'Id -> unit }

module GenericDataAccess =
    let getFileNameString filePath =
        filePath 
        |> FilePath.fileNameWithoutExtension
        |> FileName.value

    let initWithCustomId<'Id, 'Data when 'Id: equality> 
        (dirPath: DirectoryPath) 
        (fileSystem: API.FileSystem.DataAccess)
        (idToString: 'Id -> string)
        (stringToId: string -> 'Id) =
        fileSystem.Directory.EnsureExists dirPath
        let writeEvent = Event<'Id>()
        let readAll() =
            fileSystem.Directory.ListFiles dirPath
            |> Seq.filter (FilePath.hasExtension "json")
            |> Seq.map (fun filePath -> 
                fileSystem.File.ReadAllText filePath
                |> Util.Json.fromJson<'Data> )
        let filePathIdCompare requestedId filePath =
            let id = 
                getFileNameString filePath
                |> stringToId
            id = requestedId
        let getDataFilePath id =
            fileSystem.Directory.ListFiles dirPath
            |> Seq.filter (FilePath.hasExtension "json")
            |> Seq.find (filePathIdCompare id)
        let dataAccess: GenericDataAccess<'Id, 'Data> = {
            List = fun _ ->
                fileSystem.Directory.ListFiles dirPath
                |> Seq.filter (FilePath.hasExtension "json")
                |> Seq.map (fun filePath ->
                    getFileNameString filePath
                    |> stringToId)
            Read = fun id ->
                getDataFilePath id
                |> fileSystem.File.ReadAllText 
                |> Util.Json.fromJson<'Data>
            TryRead = fun id ->
                match
                    fileSystem.Directory.ListFiles dirPath
                    |> Seq.filter (FilePath.hasExtension "json")
                    |> Seq.tryFind (filePathIdCompare id)
                with
                | Some filePath ->
                    filePath
                    |> fileSystem.File.ReadAllText 
                    |> Util.Json.fromJson<'Data>
                    |> Some
                | None -> None
            ReadAll = readAll
            Write = fun id data ->
                let idString = idToString id
                data
                |> Util.Json.toJson
                |> fileSystem.File.WriteText (dirPath/FileName $"{idString}.json")
                writeEvent.Trigger id
            WriteEvent = writeEvent.Publish
            Delete = fun id -> 
                getDataFilePath id
                |> fileSystem.File.Delete
            EnsureDeleted = fun id -> 
                match
                    fileSystem.Directory.ListFiles dirPath
                    |> Seq.filter (FilePath.hasExtension "json")
                    |> Seq.tryFind (filePathIdCompare id)
                with
                | Some filePath -> fileSystem.File.Delete filePath
                | None -> ()   }
        dataAccess

    let init<'Data> (dirPath: DirectoryPath) (fileSystem: API.FileSystem.DataAccess) =
        fileSystem.Directory.EnsureExists dirPath
        initWithCustomId<System.Guid, 'Data> dirPath fileSystem (fun id -> id.ToString()) (fun idString -> System.Guid idString)
