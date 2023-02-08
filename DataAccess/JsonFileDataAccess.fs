namespace Util.DataAccess

open Util.IO.Path

type JsonFileDataAccess (dataEntriesDirPath: DirectoryPath) =
    let jsonFieldValueComparer (fieldName: string) value (jsonFilePath: FilePath) =
        let json = Util.Json.read jsonFilePath
        json.Value(fieldName) = value 

    let getEntryFilePath id =
        let fileName = FileName id |> FileName.setExtension "json"
        dataEntriesDirPath/fileName

    member this.ReadAll() =
        if not (dataEntriesDirPath |> Util.IO.Directory.exists) then Seq.empty
        else
            let dirInfo = System.IO.DirectoryInfo(dataEntriesDirPath.Value)
            dirInfo.EnumerateFiles()
            |> Seq.map (fun x -> x.FullName |> Util.IO.Path.FilePath |> Util.IO.File.readAllText)

    member this.Write (id: string) (jsonString: string) =
        let entryFilePath = getEntryFilePath id
        if entryFilePath |> Util.IO.File.exists then Util.IO.File.delete entryFilePath
        Util.IO.File.writeText entryFilePath jsonString

    member this.Delete id = getEntryFilePath id |> Util.IO.File.delete

    member this.HasFieldValue fieldName value =
        if not (dataEntriesDirPath |> Util.IO.Directory.exists) then false
        else 
            Util.IO.Directory.listFiles dataEntriesDirPath
            |> Seq.exists (jsonFieldValueComparer fieldName value)

    member this.HasId id = getEntryFilePath id |> Util.IO.File.exists

    member this.FindByFieldValue fieldName (value: 'b) =
        Util.IO.Directory.listFiles dataEntriesDirPath
        |> Seq.find (jsonFieldValueComparer fieldName value)
        |> Util.Json.deserializeFile<'Record>

    member this.FindById (id: string) = getEntryFilePath id |> Util.IO.File.readAllText

    member this.UpdateFieldValue id (fieldName: string) value =
        let entryFilePath = getEntryFilePath id
        let json = Util.Json.read entryFilePath
        json[fieldName] <- value
        Util.IO.File.writeText entryFilePath (json.ToString())
