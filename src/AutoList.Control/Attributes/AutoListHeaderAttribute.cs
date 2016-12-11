namespace AutoList.Control.Attributes
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;

   using AutoList.Control.Enums;
   using System.Windows;

   [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
   public sealed class AutoListHeaderAttribute : Attribute
   {
      /// <summary>
      /// Column header
      /// </summary>
      public string Header { get; private set; }
      
      /// <summary>
      /// Order
      /// </summary>
      public int Order { get; private set; }
      
      /// <summary>
      /// If true column will be shown as default
      /// </summary>
      public bool IsVisibleByDefault { get; set; }
      
      /// <summary>
      /// If true column will be shown in the context menu, so user can hide and unhide it in runtime
      /// </summary>
      public bool IsVisibleInContextMenu { get; set; }
      
      /// <summary>
      /// Allows user to resize column
      /// </summary>
      public bool IsResizable { get; set; }
     
      /// <summary>
      /// List's identifier. This attribute will be used to generate a column for the specified list.
      /// </summary>
      public string UniqueListIdentifier { get; set; }
     
      /// <summary>
      /// Customize how the values should be shown in this column.
      /// </summary>
      public string StringFormat { get; set; }
      
      /// <summary>
      /// Show custom value if the current value is null
      /// </summary>
      public object NullValue { get; set; }
      
      /// <summary>
      /// Column control type, default : Auto
      /// </summary>
      public AutoListColumnType ColumnType { get; set; }
      
      /// <summary>
      /// Column's startup width
      /// </summary>
      public double Width { get; set; }
      
      /// <summary>
      /// Content horizontal alignment.
      /// </summary>
      /// <remarks>
      /// Does not apply for Progressbar (always streched) and Checkbox (always center)
      /// </remarks>
      public HorizontalAlignment HorizontalConentAlignment { get; set; }

      public AutoListHeaderAttribute(string header, int order, string uniqueListIdentifier)
      {
         this.Header = header;
         this.Order = order;
         this.UniqueListIdentifier = uniqueListIdentifier;

         // initialize
         this.IsVisibleByDefault = true;
         this.IsVisibleInContextMenu = true;
         this.IsResizable = true;
         this.ColumnType = AutoListColumnType.Auto;
         this.Width = double.NaN;
         this.HorizontalConentAlignment = System.Windows.HorizontalAlignment.Right;
      }
   }
}
