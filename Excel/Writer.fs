module Util.Excel.Writer

open Util.Excel
open Util.Path

let csvToExcel (outputFilePath: FilePath) sheetName (csv: string seq seq) =
    WriterCSharp.Writer.ConvertWithOpenXml(outputFilePath.Value, sheetName, csv) |> ignore

let appendCsvToExcel (excelFilePath: FilePath) sheetName startColumn startRow (csv: string seq seq) =
    WriterCSharp.Writer2.AppendLines(excelFilePath.Value, sheetName, startColumn, startRow, csv)
