namespace AutoList.Control.Bases
{
   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Linq;
   using System.Linq.Expressions;
   using System.Text;

   using AutoList.Control.Collections;

   internal sealed class AutoListGroupDescription : GroupDescription
   {
      private Func<object, object> PropertySelector { get; set; }

      internal bool IsExpanded { get; set; }

      public AutoListGroupDescription(Func<object, object> propertySelector)
      {
         this.PropertySelector = propertySelector;
      }

      public override object GroupNameFromItem(object item, int level, System.Globalization.CultureInfo culture)
      {
         var property = this.PropertySelector((item as AutoListItem<object>).Item);

         return property;
      }      
   }
}
