using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace AutoList.Control.Extensions
{
   internal static class ItemCollectionExtension
   {
      internal static void AddRange(this ItemCollection source, IEnumerable items)
      {
         foreach (var i in items)
         {
            source.Add(i);
         }
      }
   }
}
