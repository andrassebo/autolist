namespace AutoList.Control.EventArgs
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;

   internal class SelectionChangedEventArgs<T> : EventArgs
   {
      public bool OldValue { get; private set; }

      public bool NewValue { get; private set; }

      public T Item { get; private set; }

      public SelectionChangedEventArgs(T item, bool oldValue, bool newValue)
      {
         this.Item = item;
         this.OldValue = oldValue;
         this.NewValue = newValue;   
      }
   }
}
