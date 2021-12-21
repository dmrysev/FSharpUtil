module Util.Excel.Reader

open System.IO
open System.Collections.Generic

type WorkSheet = {
    SheetName: string
    Rows: List<List<obj>> }

let parseWorkSheets (sourceFilePath: string) sheetsToSkip =
    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance)
    use stream = File.Open(sourceFilePath, FileMode.Open, FileAccess.Read)
    use reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream)
    let data = List<WorkSheet>()
    let readRows() =
        let rows = List<List<obj>>()
        while reader.Read() do
            let row = List<obj>()
            let fieldCount = reader.FieldCount - 1
            [0..fieldCount]
            |> Seq.iter (fun i -> 
                let value = reader.GetValue(i)
                row.Add value)
            rows.Add row
        rows
    let rec readWorkSheet() =
        if not (sheetsToSkip |> Seq.contains reader.Name) then
            let worksheet = {
                SheetName = reader.Name
                Rows = readRows() }
            data.Add worksheet
        if reader.NextResult() then readWorkSheet()
    readWorkSheet()
    data

let parseHeaders worksheet =
    worksheet.Rows
    |> Seq.head
    |> Seq.cast<string>

let findHeaderIndex headers headerName =
    headers
    |> Seq.findIndex (fun header -> string(header) = headerName)

let findWorkSheet worksheets name =
    worksheets
    |> Seq.find (fun worksheet -> worksheet.SheetName = name)
