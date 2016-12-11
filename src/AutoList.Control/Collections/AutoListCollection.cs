namespace AutoList.Control.Collections
{
   using System;
   using System.Collections;
   using System.Collections.Generic;
   using System.Collections.ObjectModel;
   using System.Collections.Specialized;
   using System.ComponentModel;
   using System.Linq;
   using System.Text;
   using System.Windows.Data;
   using AutoList.Control.EventArgs;

   internal class AutoListCollection<T> : IEnumerable<AutoListItem<T>>, INotifyCollectionChanged
      where T : class
   {
      private ObservableCollection<AutoListItem<T>> InternalCollection { get; set; }

      public event EventHandler<SelectedItemsChangedEventArgs<T>> SelectedItemsChanged;
      public event EventHandler ItemsPropertyChanged;
      public event NotifyCollectionChangedEventHandler CollectionChanged;

      public AutoListCollection()
      {
         this.InternalCollection = new ObservableCollection<AutoListItem<T>>();
         // bubble event
         this.InternalCollection.CollectionChanged += (s, e) =>
            {
               var handler = this.CollectionChanged;
               if (handler != null)
               {
                  handler(this, e);
               }
            };
      }

      public void Add(T item)
      {
         this.CreateItemAndAddToInternalCollection(item);

      }

      public void Add(IList items)
      {
         foreach (T i in items)
         {
            this.CreateItemAndAddToInternalCollection(i);
         }

      }

      public void Remove(IList items)
      {
         var autoListItems = this.InternalCollection.Where(i => items.Contains(i.Item)).ToList();
         foreach (var i in autoListItems)
         {
            this.InternalCollection.Remove(i);
         }
      }

      public void Clear()
      {
         this.InternalCollection.Clear();

      }

      public void Initialize(IList source)
      {
         foreach (dynamic i in source)
         {
            this.CreateItemAndAddToInternalCollection(i);
         }

      }

      private void CreateItemAndAddToInternalCollection(T item)
      {
         var autoListItem = new AutoListItem<T>(item);
         autoListItem.SelectionChanged += AutoListItemSelectionChanged;
         autoListItem.ItemsPropertyChanged += AutoListItemItemsPropertyChanged;
         this.InternalCollection.Add(autoListItem);
      }

      private void AutoListItemItemsPropertyChanged(object sender, EventArgs e)
      {
         var handler = this.ItemsPropertyChanged;
         if (handler != null)
         {
            handler(this, EventArgs.Empty);
         }
      }

      private void AutoListItemSelectionChanged(object sender, SelectionChangedEventArgs<T> e)
      {
         var handler = this.SelectedItemsChanged;
         if (handler != null)
         {
            List<T> addedItems = new List<T>();
            List<T> removedItems = new List<T>();

            if (e.NewValue)
            {
               addedItems.Add(e.Item);
            }
            else
            {
               removedItems.Add(e.Item);
            }

            handler(this, new SelectedItemsChangedEventArgs<T>(addedItems, removedItems));
         }
      }

      public IEnumerable<AutoListItem<T>> Fetch(int start, int count)
      {
         return this.InternalCollection.Skip(start).Take(count);
      }

      public IEnumerator<AutoListItem<T>> GetEnumerator()
      {
         return this.InternalCollection.GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return this.GetEnumerator();
      }

      public AutoListItem<T> this[int index]
      {
         get
         {
            return this.InternalCollection[index];
         }
      }

      public int Count
      {
         get
         {
            return this.InternalCollection.Count;
         }
      }

      public IEnumerable<AutoListItem<T>> SelectedItems
      {
         get
         {
            return this.InternalCollection.Where(i => i.IsSelected);
         }
      }
   }
}
