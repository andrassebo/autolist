namespace AutoList.Control.Attributes
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;

   [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
   public sealed class AutoListItemsSourceAttribute : Attribute
   {
      public string UniqueListIdentifier { get; private set; }

      public AutoListItemsSourceAttribute(string uniqueListIdentifier)
      {
         this.UniqueListIdentifier = uniqueListIdentifier;
      }
   }
}
