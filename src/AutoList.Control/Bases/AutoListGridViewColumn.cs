namespace AutoList.Control.Bases
{
   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Diagnostics;
   using System.Linq;
   using System.Text;
   using System.Windows.Controls;

   internal sealed class AutoListGridViewColumn : GridViewColumn
   {
      public AutoListGridViewColumn(string identifier)
      {
         this.IsSelectorColumn = false;
         this.IsResizable = true;
         this.UniqueColumnIdentifier = identifier;         
      }      

      public ListSortDirection SortDirection { get; internal set; }

      public bool IsSelectorColumn { get; internal set; }

      public bool IsResizable { get; set; }

      public string UniqueColumnIdentifier { get; internal set; }

      public double DesiredWidth { get; internal set; }

      // it's needed to store for sorting
      public string BindingPath { get; internal set; }

      private bool isVisible = true;
      public bool IsVisible
      {
         get
         {
            return this.isVisible;
         }
         set
         {
            if (value != this.isVisible)
            {
               this.isVisible = value;
               this.Width = value ? double.NaN : 0;

               this.InvalidateProperty(GridViewColumn.WidthProperty);
            }
         }
      }      

      protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
      {
         //// Use 0.0 to compare doubles with zero
         //// Do not allow to resize a visible column under 10px, so it makes sure it can be resized again and won't be invisible
         if ((this.IsVisible == false) || (e.Property == GridViewColumn.WidthProperty && ((this.IsResizable && this.IsVisible && ((double)e.NewValue > 10) || (double.IsNaN((double)e.NewValue))) || (e.NewValue.Equals(0.0) && double.IsNaN((double)e.OldValue)))))
         {
            base.OnPropertyChanged(e);
         }

         if (this.IsVisible == false)
         {
            this.Width = 0;
         }                 
      }
   }
}