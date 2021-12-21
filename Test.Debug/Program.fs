open Util
open Util.IO.Path

let csv = [
    ["Part 1"; "bzzz"] |> List.toSeq
    ["Part 2"; "rrrr"]
]
let bomTemplateFilePath = FilePath "/home/cui/tmp/bom_template.xlsx"
let testExcelFilePath = FilePath "/home/cui/tmp/test.xlsx"
IO.File.copy bomTemplateFilePath testExcelFilePath.Value
Excel.WriterCSharp.Writer2.AppendLines(testExcelFilePath.Value, "Sheet1", 'B', 12, csv)
