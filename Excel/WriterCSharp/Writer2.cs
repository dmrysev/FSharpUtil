namespace Util.Excel.WriterCSharp;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

public class Writer2
{
    // Given a document name and text, 
     // inserts a new work sheet and writes the text to cell "A1" of the new worksheet.
public static void AppendLines(string docName, string sheetName, char startColumn, int startRow, IEnumerable<IEnumerable<string>> lines)
{
    using var spreadSheet = SpreadsheetDocument.Open(docName, true);
    SharedStringTablePart shareStringPart;
    if (spreadSheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
    {
        shareStringPart = spreadSheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
    }
    else
    {
        shareStringPart = spreadSheet.WorkbookPart.AddNewPart<SharedStringTablePart>();
    }
    var worksheetPart = GetWorksheetPartBySheetName(spreadSheet.WorkbookPart, sheetName);
    var rowIndex = (uint) startRow;
    foreach(var line in lines) {
        var columnIndex = startColumn;
        foreach(var text in line) {
            int index = InsertSharedStringItem(text, shareStringPart);
            Cell cell = InsertCellInWorksheet($"{columnIndex}", rowIndex, worksheetPart);
            cell.CellValue = new CellValue(index.ToString());
            cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
            columnIndex++;
        }
        rowIndex++;
    }
    worksheetPart.Worksheet.Save();
}


private static WorksheetPart GetWorksheetPartBySheetName(WorkbookPart workbookPart, string sheetName)
{
    WorksheetPart worksheetPart = null;

    //find the sheet (note this is case-sensitive)
    IEnumerable<Sheet> sheets = workbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().Where(s => s.Name == sheetName);

    if (sheets.Count() > 0)
    {
        string relationshipId = sheets.First().Id.Value;
        worksheetPart = (WorksheetPart)workbookPart.GetPartById(relationshipId);
    }

    return worksheetPart;
}

            // Given text and a SharedStringTablePart, creates a SharedStringItem with the specified text 
            // and inserts it into the SharedStringTablePart. If the item already exists, returns its index.
            private static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
            {
                // If the part does not contain a SharedStringTable, create one.
                if (shareStringPart.SharedStringTable == null)
                {
                    shareStringPart.SharedStringTable = new SharedStringTable();
                }

                int i = 0;

                // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
                foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
                {
                    if (item.InnerText == text)
                    {
                        return i;
                    }

                    i++;
                }

                // The text does not exist in the part. Create the SharedStringItem and return its index.
                shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
                shareStringPart.SharedStringTable.Save();

                return i;
            }

            // Given a WorkbookPart, inserts a new worksheet.
            private static WorksheetPart InsertWorksheet(WorkbookPart workbookPart)
            {
                // Add a new worksheet part to the workbook.
                WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                newWorksheetPart.Worksheet = new Worksheet(new SheetData());
                newWorksheetPart.Worksheet.Save();

                Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
                string relationshipId = workbookPart.GetIdOfPart(newWorksheetPart);

                // Get a unique ID for the new sheet.
                uint sheetId = 1;
                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                string sheetName = "Sheet" + sheetId;

                // Append the new worksheet and associate it with the workbook.
                Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
                sheets.Append(sheet);
                workbookPart.Workbook.Save();

                return newWorksheetPart;
            }

            // Given a column name, a row index, and a WorksheetPart, inserts a cell into the worksheet. 
            // If the cell already exists, returns it. 
            private static Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
            {
                Worksheet worksheet = worksheetPart.Worksheet;
                SheetData sheetData = worksheet.GetFirstChild<SheetData>();
                string cellReference = columnName + rowIndex;

                // If the worksheet does not contain a row with the specified row index, insert one.
                Row row;
                if (sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() != 0)
                {
                    row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
                }
                else
                {
                    row = new Row() { RowIndex = rowIndex };
                    sheetData.Append(row);
                }

                // If there is not a cell with the specified column name, insert one.  
                if (row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
                {
                    return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
                }
                else
                {
                    // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                    Cell refCell = null;
                    foreach (Cell cell in row.Elements<Cell>())
                    {
                        if (string.Compare(cell.CellReference.Value, cellReference, true) > 0)
                        {
                            refCell = cell;
                            break;
                        }
                    }

                    Cell newCell = new Cell() { CellReference = cellReference };
                    row.InsertBefore(newCell, refCell);

                    worksheet.Save();
                    return newCell;
                }
            }
}