
namespace AutoList.Control.EventArgs
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;

   internal class SelectedItemsChangedEventArgs<T> : EventArgs
   {
      public List<T> AddedItems { get; private set; }
      public List<T> RemovedItems { get; private set; }

      public SelectedItemsChangedEventArgs(List<T> addedItems, List<T> removedItems)
      {
         this.AddedItems = addedItems;
         this.RemovedItems = removedItems;
      }
   }
}
