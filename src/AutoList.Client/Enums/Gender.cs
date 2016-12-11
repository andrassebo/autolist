namespace AutoList.Client.Enums
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;

   using AutoList.Control.Attributes;

   public enum Gender
   {
      [AutoListEnumMemberTranslator("Male", "Main")]
      Male = 1,
      [AutoListEnumMemberTranslator("Female", "Main")]
      Female = 2
   }
}
