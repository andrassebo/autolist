namespace AutoList.Control.Collections
{
   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Linq;
   using System.Text;

   using AutoList.Control.Bases;
   using AutoList.Control.EventArgs;

   /// <summary>
   /// Wrapper class for items to support selection
   /// </summary>
   /// <typeparam name="T"></typeparam>
   internal class AutoListItem<T> : Notifiable
      where T : class
   {
      public T Item { get; internal set; }

      private bool isSelected;
      public bool IsSelected
      {
         get
         {
            return this.isSelected;
         }
         set
         {
            var oldValue = this.isSelected;
            var newValue = value;

            if (oldValue != newValue)
            {
               this.isSelected = value;
               this.RaisePropertyChanged(() => this.IsSelected);

               var handler = this.SelectionChanged;
               if (handler != null)
               {
                  handler(this, new SelectionChangedEventArgs<T>(this.Item, oldValue, newValue));
               }
            }
         }
      }

      public event EventHandler<SelectionChangedEventArgs<T>> SelectionChanged;

      public event EventHandler ItemsPropertyChanged;

      public AutoListItem(T item)
      {
         this.Item = item;

         var notifiableItem = this.Item as INotifyPropertyChanged;
         if (notifiableItem != null)
         {
            notifiableItem.PropertyChanged += OnItemsPropertyChanged;
         }
      }

      private void OnItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         var handler = ItemsPropertyChanged;
         if (handler != null)
         {
            handler(this, EventArgs.Empty);
         }
      }

      public override bool Equals(object obj)
      {
         var that = obj as AutoListItem<T>;
         return that != null && that.Item == this.Item;
      }

      public override int GetHashCode()
      {
         return this.Item.GetHashCode();
      }
   }
}
