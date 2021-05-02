﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//Make sure the NuGet Package DocumentFormat.OpenXml is installed !!!
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

//Make sure to set under Debuggen->Debugeigenschaften->Anwendung-Zielframework to .NET 5 !!!

namespace Zeiss.PublicationManager.Data.Excel.IO.Read
{
    //NOTE: When we use with Console, all members have to be static !!!
    public class RowSelect : ExcelIOBase
    {       
        public static Dictionary<string, List<object>> Select(string filepath, string worksheetName, List<string> columnNames)
        {
            SpreadsheetDocument spreadsheetDocument = OpenSpreadsheetDocument(filepath, worksheetName, out SheetData sheetData, false, false);

            //Try to read SharedStringTable if it exists. If not, make sure to do NOT try to read from it
            SharedStringTable sharedStringTable = spreadsheetDocument?.WorkbookPart?.SharedStringTablePart?.SharedStringTable;

            return Reader(sheetData, sharedStringTable, GetColumnLetterIDsOfColumnNames(ref spreadsheetDocument, sheetData, columnNames, out int rowIndex), ++rowIndex);
        }

        //columnLetterIDs: <columnLetterID, columnName>
        protected static Dictionary<string, List<object>> Reader(SheetData sheetData, SharedStringTable sharedStringTable, 
            Dictionary<string, string> columnLetterIDsAndColumnNames, int startRowIndex = 1)
        {
            Dictionary<string, List<object>> rowsList = new(columnLetterIDsAndColumnNames.Select(x => new KeyValuePair<string, List<object>>(x.Value, new())));

            foreach (Row row in sheetData.Elements<Row>())
            {
                //Only read rows after index => (could be used) to prevent reading header columns
                if (row?.RowIndex?.Value >= startRowIndex)
                {
                    foreach (Cell cell in row.Elements<Cell>())
                    {
                        string columnName = columnLetterIDsAndColumnNames[GetLetterIDOfCellReference(cell.CellReference)];
                        if (rowsList.ContainsKey(columnName))
                            rowsList[columnName].Add(ReadCell(cell, sharedStringTable));
                    }                       
                }
            }

            return rowsList;
        }
    }
}
