namespace AutoList.Control.Collections
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;
   using System.Windows.Controls;

   using AutoList.Control.Attributes;
   using AutoList.Control.Bases;

   internal sealed class AutoListColumnHeaderItem
   {
      public AutoListGridViewColumn Column { get; private set; }

      public AutoListHeaderAttribute Attribute { get; private set; }

      public AutoListColumnHeaderItem(AutoListGridViewColumn column, AutoListHeaderAttribute attribute)
      {
         this.Column = column;
         this.Attribute = attribute;
      }
   }
}
