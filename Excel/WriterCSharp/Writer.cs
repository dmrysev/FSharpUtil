namespace Util.Excel.WriterCSharp;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

public class Writer
{
    public static bool ConvertWithOpenXml(string excelFileName, string worksheetName, IEnumerable<IEnumerable<string>> csvLines) {
        if (csvLines == null || csvLines.Count() == 0)
        {
            return (false);
        }
        using (SpreadsheetDocument package = SpreadsheetDocument.Create(excelFileName, SpreadsheetDocumentType.Workbook, true))
        {
            package.AddWorkbookPart();
            package.WorkbookPart.Workbook = new Workbook();
            package.WorkbookPart.AddNewPart<WorksheetPart>();
            SheetData xlSheetData = new SheetData();
            foreach (var line in csvLines)
            {
                Row xlRow = new Row();
                foreach (var col in line)
                {
                    Cell xlCell = new Cell(new InlineString(new Text(col.ToString()))) { DataType = CellValues.InlineString };
                    xlRow.Append(xlCell);
                }
                xlSheetData.Append(xlRow);
            }
            package.WorkbookPart.WorksheetParts.First().Worksheet = new Worksheet(xlSheetData);
            package.WorkbookPart.WorksheetParts.First().Worksheet.Save();

        
            // create the worksheet to workbook relation
            package.WorkbookPart.Workbook.AppendChild(new Sheets());
            package.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(new Sheet()
            {
                Id = package.WorkbookPart.GetIdOfPart(package.WorkbookPart.WorksheetParts.First()),

                SheetId = 1,

                Name = worksheetName

            });

            package.WorkbookPart.Workbook.Save();
        }
        return (true);
    }

}
