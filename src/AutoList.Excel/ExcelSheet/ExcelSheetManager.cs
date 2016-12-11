using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;

namespace AutoList.Excel.ExcelSheet
{
   using AutoList.Excel.Content;

   using DocumentFormat.OpenXml.Spreadsheet;

   internal class ExcelSheetManager
   {
      private readonly SpreadsheetDocument document = null;

      public ExcelSheetManager(SpreadsheetDocument xls)
      {
         this.document = xls;
      }

      public int AddSheet(string sheetName)
      {         
         var sheet = new Sheet
                    {
                       Id = document.WorkbookPart.GetIdOfPart(document.WorkbookPart.WorksheetParts.First()),
                       SheetId = (UInt32)this.document.WorkbookPart.Workbook.Sheets.Count() + 1,
                       Name = sheetName
                    };

         document.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(sheet);

         return (int)sheet.SheetId.Value - 1;
      }

      public void AddContent(IEnumerable<ExcelCell[]> content, IEnumerable<string> headers, int sheetIndex)
      {
         this.AddHeaderRow(headers, sheetIndex);
         this.AddRows(content, sheetIndex);         
      }

      private void AddHeaderRow(IEnumerable<string> headers, int sheetIndex)
      {
         var headerRow = new Row();
         foreach (string c in headers)
         {
            headerRow.AppendChild(new Cell
            {
               DataType = CellValues.String,
               CellValue = new CellValue(c)
            });
         }

         var sheetData = this.document.WorkbookPart.WorksheetParts.ElementAt(sheetIndex).Worksheet.GetFirstChild<SheetData>();
         sheetData.AppendChild(headerRow);
      }

      private void AddRows(IEnumerable<ExcelCell[]> content, int sheetIndex)
      {
         var sheetData = this.document.WorkbookPart.WorksheetParts.ElementAt(sheetIndex).Worksheet.GetFirstChild<SheetData>();

         foreach (var item in content)
         {
            Row row = new Row();
            foreach (var column in item)
            {               
               Cell cell = new Cell { DataType = GetCellTypeFromValue(column.Content) };
               cell.CellValue = new CellValue(column.Content == null ? string.Empty : column.Content.ToString());
               row.AppendChild(cell);
            }

            sheetData.AppendChild(row);
         }
      }

      private CellValues GetCellTypeFromValue(object value)
      {
         var result = CellValues.String;

         if (value != null)
         {
            var type = value.GetType();
            if (type == typeof(int) || type == typeof(int?))
            {
               result = CellValues.Number;
            }
         }

         return result;
      }

      public void Save()
      {
         foreach (var worksheet in document.WorkbookPart.WorksheetParts)
         {
            worksheet.Worksheet.Save();
         }

         document.WorkbookPart.Workbook.Save();         
         document.Close();
      }
   }
}
