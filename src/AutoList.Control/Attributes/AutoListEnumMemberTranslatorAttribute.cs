namespace AutoList.Control.Attributes
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;

   [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
   public sealed class AutoListEnumMemberTranslatorAttribute : Attribute
   {
      public string TranslatedText { get; private set; }

      public string UniqueListIdentifier { get; private set; }

      public AutoListEnumMemberTranslatorAttribute(string translatedText, string uniqueListIdentifier)
      {
         this.TranslatedText = translatedText;
         this.UniqueListIdentifier = uniqueListIdentifier;
      }
   }
}
