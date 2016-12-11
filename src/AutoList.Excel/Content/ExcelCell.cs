namespace AutoList.Excel.Content
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;
   
   public struct ExcelCell
   {
      public int Index { get; set; }
      public dynamic Content { get; set; }
   }
}
