namespace AutoList.Control.Configurations
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;

   using AutoList.Control.Bases;
   using AutoList.Control.Collections;
   using AutoList.Control.EventArgs;

   public sealed class AutoListColumnConfiguration
   {
      // use private to prevent add, remove
      private List<AutoListGeneratedColumn> GeneratedColumns { get; set; }

      internal event EventHandler<AutoListGeneratedColumnChangedEventArgs> ColumnChanged;

      public AutoListColumnConfiguration()
      {
         this.GeneratedColumns = new List<AutoListGeneratedColumn>();   
      }

      internal void Add(AutoListGeneratedColumn column)
      {
         column.ColumnChanged += OnColumnChanged;

         this.GeneratedColumns.Add(column);
      }

      // bubble
      private void OnColumnChanged(object sender, AutoListGeneratedColumnChangedEventArgs e)
      {
         var handler = this.ColumnChanged;
         if (handler != null)
         {            
            handler(sender, e);
         }
      }

      public AutoListGeneratedColumn GetColumn(int index)
      {
         return this.GeneratedColumns[index];
      }

      public AutoListGeneratedColumn GetColumn(string header)
      {
         return this.GeneratedColumns.Single(c => c.Header == header);
      }

      public IEnumerable<AutoListGeneratedColumn> GetColumns(Func<AutoListGeneratedColumn, bool> predicate)
      {
         return this.GeneratedColumns.Where(predicate);
      }
   }
}
