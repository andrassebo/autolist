namespace AutoList.Control.Converters
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Reflection;
   using System.Text;
   using System.Windows.Data;

   using AutoList.Control.Attributes;

   internal class TranslatorConverter : IValueConverter
   {
      private Type EnumType { get; set; }

      private string UniqueListIdentifier { get; set; }

      public TranslatorConverter(Type enumType, string uniqueListIdentifier)
      {
         this.EnumType = enumType;
         this.UniqueListIdentifier = uniqueListIdentifier;
      }

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         var newValue = value;

         var fields = EnumType.GetFields();

         foreach (var f in fields.Where(f => f.IsLiteral))
         {
            if (f.GetValue(f).Equals(value))
            {
               var attributes = f.GetCustomAttributes(typeof(AutoListEnumMemberTranslatorAttribute), false) as AutoListEnumMemberTranslatorAttribute[];

               if (attributes != null)
               {
                  var attribute = attributes.SingleOrDefault(a => a.UniqueListIdentifier == this.UniqueListIdentifier);

                  if (attribute != null)
                  {
                     newValue = attribute.TranslatedText;
                  }                  
               }

               // we have found it
               break;
            }
         }

         return newValue;
      }

      // TODO : implement convert back      
      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return value;
      }
   }
}
