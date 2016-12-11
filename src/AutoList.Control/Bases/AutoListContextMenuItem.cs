namespace AutoList.Control.Bases
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;
   using System.Windows.Controls;

   internal sealed class AutoListContextMenuItem : MenuItem
   {
      public string UniqueColumnIdentifier { get; set; }

      public AutoListContextMenuItem(string columnIdentifier)
      {
         this.UniqueColumnIdentifier = columnIdentifier;
      }
   }
}
