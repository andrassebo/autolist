using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AutoList.Excel.ExcelSheet;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace AutoList.Excel
{
   using AutoList.Excel.Content;

   public class ExcelExporter
   {
      private IEnumerable<ExcelCell[]> Items { get; set; }
      private IEnumerable<string> Headers { get; set; }

      public ExcelExporter(IEnumerable<ExcelCell[]> items, IEnumerable<string> headers)
      {
         this.Items = items;
         this.Headers = headers;
      }

      public bool Save(string fileName)
      {
         var xls = this.CreateExcel(fileName);
         var sheetManager = new ExcelSheetManager(xls);
         var sheetIndex = sheetManager.AddSheet("AutoList");
         sheetManager.AddContent(this.Items, this.Headers, sheetIndex);

         sheetManager.Save();

         return true;
      }

      private SpreadsheetDocument CreateExcel(string fileName)
      {
         SpreadsheetDocument spreadSheet = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);

         spreadSheet.AddWorkbookPart();
         spreadSheet.WorkbookPart.Workbook = new Workbook();

         spreadSheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

         WorkbookStylesPart workbookStylesPart = spreadSheet.WorkbookPart.AddNewPart<WorkbookStylesPart>();
         Stylesheet stylesheet = new Stylesheet();
         workbookStylesPart.Stylesheet = stylesheet;

         WorksheetPart newWorksheetPart = spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();
         spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet = new Worksheet();

         spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.AppendChild(new SheetData());
         spreadSheet.WorkbookPart.Workbook.AppendChild(new Sheets());            

         newWorksheetPart.Worksheet.Save();         

         return spreadSheet;
      }

   }
}
