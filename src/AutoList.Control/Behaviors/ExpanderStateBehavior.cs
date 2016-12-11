//
// This behavior is from http://stackoverflow.com/questions/2808777/how-to-save-the-isexpanded-state-in-group-headers-of-a-listview 
//
namespace AutoList.Control.Behaviors
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;
   using System.Windows;
   using System.Windows.Controls;
   using System.Windows.Interactivity;
   using System.Windows.Media;

   public class PersistGroupExpandedStateBehavior : Behavior<Expander>
   {      
      public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(
         "GroupName", typeof(object), typeof(PersistGroupExpandedStateBehavior), new PropertyMetadata(default(object)));

      private static readonly DependencyProperty ExpandedStateStoreProperty = DependencyProperty.RegisterAttached(
         "ExpandedStateStore", typeof(IDictionary<object, bool>), typeof(PersistGroupExpandedStateBehavior), new PropertyMetadata(default(IDictionary<object, bool>)));

      public object GroupName
      {
         get { return (object)this.GetValue(GroupNameProperty); }
         set { this.SetValue(GroupNameProperty, value); }
      }            

      protected override void OnAttached()
      {
         base.OnAttached();

         bool? expanded = this.GetExpandedState();

         if (expanded != null)
         {
            this.AssociatedObject.IsExpanded = expanded.Value;
         }

         this.AssociatedObject.Expanded += this.OnExpanded;
         this.AssociatedObject.Collapsed += this.OnCollapsed;
      }

      protected override void OnDetaching()
      {
         this.AssociatedObject.Expanded -= this.OnExpanded;
         this.AssociatedObject.Collapsed -= this.OnCollapsed;

         base.OnDetaching();
      }

      private ItemsControl FindItemsControl()
      {
         DependencyObject current = this.AssociatedObject;

         while (current != null && (current is ItemsControl) == false)
         {
            current = VisualTreeHelper.GetParent(current);
         }

         if (current == null)
         {
            return null;
         }

         return current as ItemsControl;
      }

      private bool? GetExpandedState()
      {
         var dict = this.GetExpandedStateStore();

         if (this.GroupName == null || dict.ContainsKey(this.GroupName) == false)
         {
            return null;
         }

         return dict[this.GroupName];
      }

      private IDictionary<object, bool> GetExpandedStateStore()
      {
         ItemsControl itemsControl = this.FindItemsControl();

         if (itemsControl == null)
         {
            throw new Exception("Behavior needs to be attached to an Expander that is contained inside an ItemsControl");
         }

         var dict = (IDictionary<object, bool>)itemsControl.GetValue(ExpandedStateStoreProperty);

         if (dict == null)
         {
            dict = new Dictionary<object, bool>();
            itemsControl.SetValue(ExpandedStateStoreProperty, dict);
         }

         return dict;
      }

      private void OnCollapsed(object sender, RoutedEventArgs e)
      {
         this.SetExpanded(false);
      }

      private void OnExpanded(object sender, RoutedEventArgs e)
      {
         this.SetExpanded(true);
      }

      private void SetExpanded(bool expanded)
      {
         var dict = this.GetExpandedStateStore();

         dict[this.GroupName] = expanded;
      }
   }
}
