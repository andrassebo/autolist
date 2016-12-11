namespace AutoList.Control.EventArgs
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;

   using AutoList.Control.Configurations;

   internal sealed class AutoListGeneratedColumnChangedEventArgs: EventArgs
   {
      public string PropertyName { get; private set; }

      public dynamic OldValue { get; private set; }

      public dynamic NewValue { get; private set; }

      internal string UniqueColumnIdentifier { get; set; }

      public AutoListGeneratedColumnChangedEventArgs(string propertyName, dynamic oldValue, dynamic newValue)
      {
         this.PropertyName = propertyName;
         this.NewValue = newValue;
         this.OldValue = oldValue;
      }
   }
}
