namespace AutoList.Control.Configurations
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;

   using AutoList.Control.Bases;
   using AutoList.Control.EventArgs;

   public sealed class AutoListGeneratedColumn : Notifiable
   {
      internal event EventHandler<AutoListGeneratedColumnChangedEventArgs> ColumnChanged;

      private string header;
      public string Header
      {
         get
         {
            return this.header;
         }
         set
         {
            this.SetAndRaise(() => this.Header, ref header, value);
         }
      }

      private bool isResizable;
      public bool IsResizable 
      {
         get
         {
            return this.isResizable;
         }
         set
         {
            this.SetAndRaise(() => this.IsResizable, ref isResizable, value);
         }
      }

      private bool isVisible;
      public bool IsVisible
      {
         get
         {
            return this.isVisible;
         }
         set
         {
            this.SetAndRaise(() => this.IsVisible, ref isVisible, value);
         }
      }

      private string UniqueColumnIdentifier { get; set; }

      public AutoListGeneratedColumn(string uniqueColumnIdentifier, string header, bool isResizable, bool isVisible)
      {         
         this.Header = header;
         this.IsResizable = isResizable;
         this.IsVisible = isVisible;
         this.UniqueColumnIdentifier = uniqueColumnIdentifier;
      }

      protected override void OnPropertyChanged<TProperty>(string propertyName, TProperty oldValue, TProperty newValue)
      {
         var handler = this.ColumnChanged;
         if (handler != null)
         {
            handler(this, new AutoListGeneratedColumnChangedEventArgs(propertyName, oldValue, newValue) { UniqueColumnIdentifier = this.UniqueColumnIdentifier });
         }
      }
   }
}
