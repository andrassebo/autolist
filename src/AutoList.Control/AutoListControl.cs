using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using AutoList.Control.Attributes;
using AutoList.Control.Bases;
using AutoList.Control.Collections;
using AutoList.Control.Configurations;
using AutoList.Control.Converters;
using AutoList.Control.Enums;
using AutoList.Control.EventArgs;
using AutoList.Excel;
using AutoList.Excel.Content;
using System.Threading;

namespace AutoList.Control
{
   [TemplatePart(Name = "PART_ListView", Type = typeof(ListView))]
   [TemplatePart(Name = "PART_GridView", Type = typeof(GridView))]
   [TemplatePart(Name = "PART_HeaderContextMenu", Type = typeof(ContextMenu))]
   [DebuggerDisplay("AutoList, UniqueListIdentifier={uniqueListIdentifier}, ItemsCount={this.ItemsSource.Count}")]
   public sealed class AutoListControl : System.Windows.Controls.Control
   {
      private ListView internalListView;
      private GridView internalGridView;
      private ContextMenu internalHeaderContextMenu;

      // obligatory, but empty as default
      private string uniqueListIdentifier = string.Empty;

      // generate columns only once
      private volatile bool isGenerated = false;

      // avoid collection change during collection event
      private bool supressSelectedItemsChanged = false;

      // avoid double invalidate
      private bool supressHighlightedItemChanged = false;

      private AutoListCollection<object> InternalItemsSource { get; set; }

      // external resources
      private ResourceDictionary AutoListResources { get; set; }

      public object InternalFocusedItem
      {
         get
         {
            return this.FocusedItem == null ? null : this.InternalItemsSource.SingleOrDefault(i => i.Item.Equals(this.FocusedItem));
         }
         set
         {
            this.supressHighlightedItemChanged = true;
            this.FocusedItem = value == null ? null : (value as AutoListItem<object>).Item;
            this.supressHighlightedItemChanged = false;
         }
      }

      static AutoListControl()
      {
         DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoListControl), new FrameworkPropertyMetadata(typeof(AutoListControl)));
      }

      public override void OnApplyTemplate()
      {
         // load external resources
         Uri resourceLocater = new Uri("/AutoList.Control;component/AutoListResources.xaml", System.UriKind.Relative);
         this.AutoListResources = (ResourceDictionary)Application.LoadComponent(resourceLocater);

         this.internalListView = this.GetTemplateChild("PART_ListView") as ListView;
         this.internalGridView = this.GetTemplateChild("PART_GridView") as GridView;
         this.internalHeaderContextMenu = this.GetTemplateChild("PART_HeaderContextMenu") as ContextMenu;

         // add custom handler to support sorting
         this.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(this.OnHeaderClicked));

         // item double click (ItemContainerStyle is null, so it is not an option here)
         this.internalListView.MouseDoubleClick += OnListItemDoubleClicked;

         // context menu
         this.ContextMenuOpening += OnAutoListContextMenuOpening;

         // entry point
         this.Loaded += OnAutoListLoaded;
      }

      #region dependency properties

      public IList ItemsSource
      {
         get { return (IList)GetValue(ItemsSourceProperty); }
         set { SetValue(ItemsSourceProperty, value); }
      }

      public static readonly DependencyProperty ItemsSourceProperty =
          DependencyProperty.Register("ItemsSource", typeof(IList), typeof(AutoListControl), new UIPropertyMetadata(null, OnItemsSourcePropertyChanged));

      public AutoListSelectionMode SelectionMode
      {
         get { return (AutoListSelectionMode)GetValue(SelectionModeProperty); }
         set { SetValue(SelectionModeProperty, value); }
      }

      public static readonly DependencyProperty SelectionModeProperty =
          DependencyProperty.Register("SelectionMode", typeof(AutoListSelectionMode), typeof(AutoListControl), new UIPropertyMetadata(AutoListSelectionMode.None));

      public IList SelectedItems
      {
         get { return (IList)GetValue(SelectedItemsProperty); }
         set { SetValue(SelectedItemsProperty, value); }
      }

      public static readonly DependencyProperty SelectedItemsProperty =
          DependencyProperty.Register("SelectedItems", typeof(IList), typeof(AutoListControl), new UIPropertyMetadata(null, OnSelectedItemsPropertyChanged));

      public bool AllowsColumnHide
      {
         get { return (bool)GetValue(AllowsColumnHideProperty); }
         set { SetValue(AllowsColumnHideProperty, value); }
      }

      public static readonly DependencyProperty AllowsColumnHideProperty =
          DependencyProperty.Register("AllowsColumnHide", typeof(bool), typeof(AutoListControl), new UIPropertyMetadata(true, OnAllowsColumnHideChanged));

      public bool AllowsColumnReorder
      {
         get { return (bool)GetValue(AllowsColumnReorderProperty); }
         set { SetValue(AllowsColumnReorderProperty, value); }
      }

      public static readonly DependencyProperty AllowsColumnReorderProperty =
          DependencyProperty.Register("AllowsColumnReorder", typeof(bool), typeof(AutoListControl), new UIPropertyMetadata(true));

      public bool IsBusy
      {
         get { return (bool)GetValue(IsBusyProperty); }
         set { SetValue(IsBusyProperty, value); }
      }

      public static readonly DependencyProperty IsBusyProperty =
          DependencyProperty.Register("IsBusy", typeof(bool), typeof(AutoListControl), new UIPropertyMetadata(false));


      public Action<AutoListColumnConfiguration> OnGenerated
      {
         get { return (Action<AutoListColumnConfiguration>)GetValue(OnGeneratedProperty); }
         set { SetValue(OnGeneratedProperty, value); }
      }

      public static readonly DependencyProperty OnGeneratedProperty =
          DependencyProperty.Register("OnGenerated", typeof(Action<AutoListColumnConfiguration>), typeof(AutoListControl), new UIPropertyMetadata(null));

      public object FocusedItem
      {
         get { return (object)GetValue(FocusedItemProperty); }
         set { SetValue(FocusedItemProperty, value); }
      }

      public static readonly DependencyProperty FocusedItemProperty =
          DependencyProperty.Register("FocusedItem", typeof(object), typeof(AutoListControl), new UIPropertyMetadata(null, OnFocusedItemChanged));


      public Action<object> OnItemDoubleClick
      {
         get { return (Action<object>)GetValue(OnItemDoubleClickProperty); }
         set { SetValue(OnItemDoubleClickProperty, value); }
      }

      public static readonly DependencyProperty OnItemDoubleClickProperty =
          DependencyProperty.Register("OnItemDoubleClick", typeof(Action<object>), typeof(AutoListControl), new UIPropertyMetadata(null));

      public Predicate<object> FilterPredicate
      {
         get { return (Predicate<object>)GetValue(FilterPredicateProperty); }
         set { SetValue(FilterPredicateProperty, value); }
      }

      public static readonly DependencyProperty FilterPredicateProperty =
          DependencyProperty.Register("FilterPredicate", typeof(Predicate<object>), typeof(AutoListControl), new UIPropertyMetadata(null, OnFilterPredicateChanged));


      public Expression<Func<object, object>> GroupByExpression
      {
         get { return (Expression<Func<object, object>>)GetValue(GroupByExpressionProperty); }
         set { SetValue(GroupByExpressionProperty, value); }
      }

      public static readonly DependencyProperty GroupByExpressionProperty =
          DependencyProperty.Register("GroupByExpression", typeof(Expression<Func<object, object>>), typeof(AutoListControl), new UIPropertyMetadata(null, OnGroupByExpressionChanged));

      public bool AutoColumnSize
      {
         get { return (bool)GetValue(AutoColumnSizeProperty); }
         set { SetValue(AutoColumnSizeProperty, value); }
      }

      public static readonly DependencyProperty AutoColumnSizeProperty =
          DependencyProperty.Register("AutoColumnSize", typeof(bool), typeof(AutoListControl), new PropertyMetadata(false, OnAutoColumnSizeChanged));

      public bool AllowExcelExport
      {
         get { return (bool)GetValue(AllowExcelExportProperty); }
         set { SetValue(AllowExcelExportProperty, value); }
      }

      public static readonly DependencyProperty AllowExcelExportProperty =
          DependencyProperty.Register("AllowExcelExport", typeof(bool), typeof(AutoListControl), new PropertyMetadata(true, OnAllowExcelExportChanged));

      // this property does not support runtime changes
      public bool ShowColumnLines
      {
         get { return (bool)GetValue(ShowColumnLinesProperty); }
         set { SetValue(ShowColumnLinesProperty, value); }
      }

      public static readonly DependencyProperty ShowColumnLinesProperty =
          DependencyProperty.Register("ShowColumnLines", typeof(bool), typeof(AutoListControl), new PropertyMetadata(false));
            
      #endregion

      // if it's loaded we are ready to start work
      private void OnAutoListLoaded(object sender, RoutedEventArgs e)
      {
         var autoList = sender as AutoListControl;
         if (autoList != null && autoList.ItemsSource != null)
         {
            this.uniqueListIdentifier = this.GetItemsSourceIdentifier();

            // unsubscribe if it wasn't null before
            if (this.InternalItemsSource != null)
            {
               this.InternalItemsSource.SelectedItemsChanged -= OnInternalItemsSourceSelectedItemsChanged;
               this.InternalItemsSource.ItemsPropertyChanged -= OnInternalItemsSourceItemsPropertyChanged;
            }

            // create internal items source to wrap items, wrapped items help us to support lot of features without to force developers to implement anything
            this.InternalItemsSource = new AutoListCollection<object>();
            this.InternalItemsSource.Initialize(autoList.ItemsSource);
            
            this.InternalItemsSource.SelectedItemsChanged += OnInternalItemsSourceSelectedItemsChanged;            
            this.InternalItemsSource.ItemsPropertyChanged += OnInternalItemsSourceItemsPropertyChanged;

            // generate columns and context menu
            if (this.InternalItemsSource.Any() && this.isGenerated == false)
            {
               this.GenerateColumnHeaders(this.InternalItemsSource[0]); 
            }

            // if it is an ObservableCollection or anything which implement this interface we need to keep items synced
            var notifiableItemSource = (this.ItemsSource as INotifyCollectionChanged);
            if (notifiableItemSource != null)
            {
               notifiableItemSource.CollectionChanged -= OnItemsSourceCollectionChanged;

               // bind it back            
               // somehow the first binding is extremly slow                   
               this.internalListView.ItemsSource = this.InternalItemsSource;

               notifiableItemSource.CollectionChanged += OnItemsSourceCollectionChanged;
            }
            else
            {
               throw new ArgumentException("ItemsSource property does not implement INotifyCollectionChanged interface.");
            }

            // check SelectedItems type
            var notifiableSelectedItems = this.SelectedItems as INotifyCollectionChanged;
            if (notifiableSelectedItems != null)
            {
               notifiableSelectedItems.CollectionChanged -= OnSelectedItemsSourceCollectionChanged;
               notifiableSelectedItems.CollectionChanged += OnSelectedItemsSourceCollectionChanged;
            }
            else if (this.SelectedItems != null)
            {
               throw new ArgumentException("SelectedItems property does not implement INotifyCollectionChanged interface.");
            }

            // apply filter
            autoList.OnApplyFilter(autoList.FilterPredicate);

            // apply groupping
            autoList.OnApplyGroupping(autoList.GroupByExpression);
         }
      }

      private string GetItemsSourceIdentifier()
      {
         string identifier = string.Empty;

         var binding = this.GetBindingExpression(ItemsSourceProperty);
         if (binding != null && binding.DataItem != null && binding.ParentBinding != null)
         {
            var attribute = binding.DataItem.GetType().GetProperty(binding.ParentBinding.Path.Path).GetCustomAttributes(typeof(AutoListItemsSourceAttribute), false).FirstOrDefault();
            if (attribute != null && attribute is AutoListItemsSourceAttribute)
            {
               identifier = (attribute as AutoListItemsSourceAttribute).UniqueListIdentifier;
            }
            else
            {
               throw new ArgumentNullException(string.Format("Could not determine UniqueListIdentifier on {0} property in {1} view model. Probably you forget to decorate this property with AutoListItemSourceAttribute.", binding.ParentBinding.Path.Path, binding.DataItem), new Exception());
            }
         }

         return identifier;
      }

      private void OnSelectedItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if (this.isGenerated && this.supressSelectedItemsChanged == false)
         {
            this.supressSelectedItemsChanged = true;
            switch (e.Action)
            {
               case NotifyCollectionChangedAction.Add:
                  var addedItems = this.InternalItemsSource.Where(i => e.NewItems.Contains(i.Item));
                  foreach (var i in addedItems)
                  {
                     i.IsSelected = true;
                  }
                  break;
               case NotifyCollectionChangedAction.Remove:
                  var removedItems = this.InternalItemsSource.Where(i => e.OldItems.Contains(i.Item));
                  foreach (var i in removedItems)
                  {
                     i.IsSelected = false;
                  }
                  break;
               case NotifyCollectionChangedAction.Reset:
                  foreach (var i in this.InternalItemsSource)
                  {
                     i.IsSelected = false;
                  }
                  break;
            }
            this.supressSelectedItemsChanged = false;
         }
      }

      private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if (this.isGenerated)
         {
            switch (e.Action)
            {
               // remove items from selected items collection and from the internal items source
               case NotifyCollectionChangedAction.Remove:
                  this.RemoveItemsFromInternalItemsSource(e.OldItems);
                  this.RemoveItemsFromSelectedItems(e.OldItems);
                  break;

               // add items to the internal items source
               case NotifyCollectionChangedAction.Add:
                  this.AddItemsToInternalItemsSource(e.NewItems);
                  break;

               case NotifyCollectionChangedAction.Reset:
                  this.InternalItemsSource.Clear();
                  if (this.SelectedItems != null)
                  {
                     this.SelectedItems.Clear();
                  }
                  break;
            }
         }
         else
         {
            this.OnAutoListLoaded(this, new RoutedEventArgs());
         }         
      }

      // refresh the list
      private void RefreshListView()
      {
         var view = CollectionViewSource.GetDefaultView(this.internalListView.ItemsSource);
         view.Refresh();
      }

      private void OnInternalItemsSourceItemsPropertyChanged(object sender, System.EventArgs e)
      {
         this.RefreshListView();
      }

      // some items were selected some of them deselected, keep our lists synced
      private void OnInternalItemsSourceSelectedItemsChanged(object sender, SelectedItemsChangedEventArgs<object> e)
      {
         if (this.SelectedItems != null && this.supressSelectedItemsChanged == false)
         {
            this.supressSelectedItemsChanged = true;
            this.AddItemsToSelectedItems(e.AddedItems);
            this.RemoveItemsFromSelectedItems(e.RemovedItems);
            this.supressSelectedItemsChanged = false;
         }
      }

      private void AddItemsToInternalItemsSource(IList items)
      {
         this.InternalItemsSource.Add(items);
      }

      private void RemoveItemsFromInternalItemsSource(IList items)
      {
         this.InternalItemsSource.Remove(items);
      }

      private void AddItemsToSelectedItems(IList items)
      {
         if (this.SelectedItems != null)
         {
            foreach (var i in items)
            {
               this.SelectedItems.Add(i);
            }
         }
      }

      private void RemoveItemsFromSelectedItems(IList items)
      {
         if (this.SelectedItems != null)
         {
            foreach (var i in items)
            {
               this.SelectedItems.Remove(i);
            }
         }
      }
      protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
      {
         base.OnRenderSizeChanged(sizeInfo);

         if (this.AutoColumnSize)
         {
            this.StretchGridViewColumns(this.internalListView.ActualWidth);
         }
      }

      private static void OnAutoColumnSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
      {
         var autoList = obj as AutoListControl;
         if (autoList != null && autoList.internalListView != null && e.NewValue != null && (bool)e.NewValue)
         {
            autoList.StretchGridViewColumns(autoList.internalListView.ActualWidth);
         }
      }

      private static void OnAllowExcelExportChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
      {
         var autoList = obj as AutoListControl;
         if (autoList != null && autoList.internalHeaderContextMenu != null && autoList.internalHeaderContextMenu.Items.Count > 0)
         {
            (autoList.internalHeaderContextMenu.Items[0] as MenuItem).Visibility = e.NewValue != null && (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
         }
      }

      private static void OnGroupByExpressionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
      {
         var autoList = obj as AutoListControl;
         if (autoList != null)
         {
            autoList.OnApplyGroupping(e.NewValue as Expression<Func<object, object>>);
         }
      }

      private void OnApplyGroupping(Expression<Func<object, object>> groupByExpression)
      {
         var view = CollectionViewSource.GetDefaultView(this.internalListView.ItemsSource);

         if (view != null)
         {
            view.GroupDescriptions.Clear();
            if (groupByExpression != null)
            {
               view.GroupDescriptions.Add(new AutoListGroupDescription(groupByExpression.Compile()));

               if (this.internalListView.GroupStyle.Count == 0)
               {
                  // using grouping makes AutoList extreme slow (in .NET4 virtualization is not available for grouping), 
                  // so it is not in default control style, but append later
                  // For better performance either use .NET4.5 or imlement ListCollectionView with better grouping performance and use it as default view. 
                  // http://code.msdn.microsoft.com/Grouping-and-Virtualization-56e7d3fe/sourcecode?fileId=18860&pathId=397578265
                  var groupStyle = this.AutoListResources["DefaultStyle"] as Style;
                  this.internalListView.GroupStyle.Add(new GroupStyle { ContainerStyle = groupStyle });
               }
            }
            else
            {
               this.internalListView.GroupStyle.Clear();
            }
         }
      }

      private static void OnFilterPredicateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
      {
         var autoList = obj as AutoListControl;
         if (autoList != null && autoList.isGenerated)
         {
            autoList.OnApplyFilter(e.NewValue as Predicate<object>);
         }
      }

      private void OnApplyFilter(Predicate<object> predicate)
      {
         var view = CollectionViewSource.GetDefaultView(this.internalListView.ItemsSource);

         if (view != null && predicate != null)
         {
            view.Filter = i => predicate((i as AutoListItem<object>).Item);
         }
      }

      private static void OnFocusedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
      {
         var autolist = obj as AutoListControl;
         if (autolist != null && autolist.supressHighlightedItemChanged == false)
         {
            if (e.NewValue != null)
            {
               autolist.internalListView.SelectedItem = autolist.InternalItemsSource.FirstOrDefault(i => i.Item.Equals(e.NewValue));
               var item = autolist.internalListView.ItemContainerGenerator.ContainerFromItem(autolist.InternalItemsSource.SingleOrDefault(c => c.Item.Equals(e.NewValue)));
               if (item != null && item is ListViewItem)
               {
                  (item as ListViewItem).IsSelected = true;
               }
            }
            else
            {
               autolist.internalListView.SelectedItem = null;
            }            
         }
      }

      // enable or disable context menu
      // supports binding and runtime changes as well
      private static void OnAllowsColumnHideChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
      {
         var autoList = obj as AutoListControl;
         if (autoList != null && e.NewValue != null && autoList.internalHeaderContextMenu != null)
         {
            autoList.ToggleContextMenuItems();
         }
      }

      private void ToggleContextMenuItems()
      {         
         foreach (var menuItem in this.internalHeaderContextMenu.Items)
         {
            var item = menuItem as AutoListContextMenuItem;
            if (item != null)
            {
               item.Visibility = this.AllowsColumnHide ? Visibility.Visible : Visibility.Collapsed;
            }
         }         
      }

      private void ToggleExcelExportMenu()
      {
         (this.internalHeaderContextMenu.Items[0] as MenuItem).Visibility = this.AllowExcelExport ? Visibility.Visible : Visibility.Collapsed;
      }

      private static void OnAutoListContextMenuOpening(object sender, ContextMenuEventArgs e)
      {
         var autoList = sender as AutoListControl;
         if (autoList != null)
         {
            e.Handled = (autoList.AllowExcelExport == false && autoList.AllowsColumnHide == false) || autoList.internalHeaderContextMenu == null || autoList.internalHeaderContextMenu.Items.Count == 0;
         }
      }

      private static void OnItemsSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
      {
         var autoList = obj as AutoListControl;
         // if loaded and items source was changed then re-generate
         if (autoList != null && autoList.IsLoaded)
         {
            if (autoList.SelectedItems != null)
            {
               autoList.SelectedItems.Clear();
            }
            autoList.OnAutoListLoaded(autoList, new RoutedEventArgs());
         }
      }

      private static void OnSelectedItemsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
      {
         // placeholder
      }

      // on header clicked, resort items
      private void OnHeaderClicked(object sender, RoutedEventArgs e)
      {
         var source = e.OriginalSource as GridViewColumnHeader;
         if (source != null && source.Column != null)
         {
            var view = CollectionViewSource.GetDefaultView(this.internalListView.ItemsSource);
            if (view != null)
            {
               var column = source.Column as AutoListGridViewColumn;
               if (column != null && string.IsNullOrWhiteSpace(column.BindingPath) == false)
               {                  
                  view.SortDescriptions.Clear();
                  // toggle sorting
                  column.SortDirection = column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                  var newSort = new SortDescription(column.BindingPath, column.SortDirection);
                  view.SortDescriptions.Add(newSort);
                  view.Refresh();
               }
            }
         }
      }

      private FrameworkElementFactory AppendColumnSeparatorBorder(FrameworkElementFactory innerContent)
      {         
         var border = new FrameworkElementFactory(typeof(Border));
         if (this.ShowColumnLines)
         {
            var brushConverter = new BrushConverter();

            // official: linear brush #FFF2F2F2;0 #FFD5D5D5;1
            border.SetValue(BorderBrushProperty, (Brush)brushConverter.ConvertFrom("#FFD5D5D5"));
            border.SetValue(BorderThicknessProperty, new Thickness(0.5, 0, 0, 0));
            border.SetValue(MarginProperty, new Thickness(-7.5, -16, 0, -16));

            innerContent.SetValue(MarginProperty, new Thickness(7.5, 16, 0, 16));
         }

         border.AppendChild(innerContent);

         return border;
      }

      // create checkbox or radio button depends on configuration
      private DataTemplate CreateSelectorControl<TToggle>(string bindingPath, bool oneWay, params KeyValuePair<DependencyProperty, object>[] customSettings)
         where TToggle : ToggleButton
      {
         DataTemplate template = new DataTemplate();        
         var factory = new FrameworkElementFactory(typeof(TToggle));    
         factory.SetBinding(ToggleButton.IsCheckedProperty, new Binding(bindingPath) { Mode = oneWay ? BindingMode.OneWay : BindingMode.TwoWay });

         // in case of radio button a group must be defined to prevent multiple item selection
         if (typeof(TToggle) == typeof(RadioButton))
         {
            factory.SetValue(RadioButton.GroupNameProperty, "__AutoListSelectorGroup");
         }

         var border = AppendColumnSeparatorBorder(this.ApplyCustomSettings(factory, customSettings));

         // apply custom settings
         template.VisualTree = border;

         template.Seal();

         return template;
      }
     
      // create a progressbar control
      private DataTemplate CreateProgressBarControl(string bindingPath, params KeyValuePair<DependencyProperty, object>[] customSettings)
      {
         DataTemplate template = new DataTemplate();         
         var factory = new FrameworkElementFactory(typeof(ProgressBar));         
         factory.SetBinding(RangeBase.ValueProperty, new Binding(bindingPath) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
         factory.SetValue(MinHeightProperty, 10.0);

         var border = AppendColumnSeparatorBorder(this.ApplyCustomSettings(factory, customSettings));
         template.VisualTree = border;

         template.Seal();

         return template;
      }

      private DataTemplate CreateTextControl(PropertyInfo propertyInfo, AutoListHeaderAttribute headerAttribute, params KeyValuePair<DependencyProperty, object>[] customSettings)
      {
         DataTemplate template = new DataTemplate();         
         var factory = new FrameworkElementFactory(typeof(TextBlock));
         factory.SetBinding(TextBlock.TextProperty, CreateBindingForColumn(propertyInfo, headerAttribute));
         factory.SetValue(HorizontalAlignmentProperty, headerAttribute.HorizontalConentAlignment);

         var border = AppendColumnSeparatorBorder(this.ApplyCustomSettings(factory, customSettings));
         template.VisualTree = border;

         template.Seal();

         return template;
      }

      private FrameworkElementFactory ApplyCustomSettings(FrameworkElementFactory factory, params KeyValuePair<DependencyProperty, object>[] customSettings)
      {
         foreach (var c in customSettings)
         {
            factory.SetValue(c.Key, c.Value);
         }

         return factory;
      }

      // create a control based on attribute
      private AutoListGridViewColumn CreateColumn(string identifier, PropertyInfo propertyInfo, AutoListHeaderAttribute headerAttribute)
      {
         var column = new AutoListGridViewColumn(identifier);
         column.Header = headerAttribute.Header;
         column.Width = headerAttribute.Width;
         column.IsResizable = headerAttribute.IsResizable;
         column.IsVisible = headerAttribute.IsVisibleByDefault;
         column.DesiredWidth = double.NaN;
         column.BindingPath = "Item." + propertyInfo.Name;

         switch (headerAttribute.ColumnType)
         {
            case AutoListColumnType.Auto:
               column.CellTemplate = CreateTextControl(propertyInfo, headerAttribute);
               break;
            case AutoListColumnType.Checkbox:
               // check nullable
               var isNullable = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
               column.CellTemplate = CreateSelectorControl<CheckBox>(
                  "Item." + propertyInfo.Name,
                  true,
                  new KeyValuePair<DependencyProperty, object>(IsEnabledProperty, false),
                  new KeyValuePair<DependencyProperty, object>(CheckBox.IsThreeStateProperty, isNullable),
                  new KeyValuePair<DependencyProperty, object>(HorizontalAlignmentProperty, HorizontalAlignment.Center)
                  );
               break;
            case AutoListColumnType.ProgressBar:
               column.CellTemplate = CreateProgressBarControl(
                  "Item." + propertyInfo.Name,
                  new KeyValuePair<DependencyProperty, object>(RangeBase.MinimumProperty, 0.0),
                  new KeyValuePair<DependencyProperty, object>(RangeBase.MaximumProperty, 100.0));
               break;
         }

         return column;         
      }

      private IEnumerable<AutoListColumnHeaderItem> CreateHeadersFromMetaItem(AutoListItem<object> metaItem)
      {         
         var properties = metaItem.Item.GetType().GetProperties();

         // find properties on list item type
         foreach (var p in properties)
         {
            var attributes = p.GetCustomAttributes(typeof(AutoListHeaderAttribute), false) as AutoListHeaderAttribute[];
            if (attributes != null)
            {
               // check list identifier
               var attribute = attributes.FirstOrDefault(a => a.UniqueListIdentifier == this.uniqueListIdentifier);

               if (attribute != null)
               {
                  var column = this.CreateColumn(p.Name, p, attribute);

                  yield return new AutoListColumnHeaderItem(column, attribute);
               }
            }
         }         
      }

      // generate columns based on meta item
      private void GenerateColumnHeaders(AutoListItem<object> metaItem)
      {
         if (metaItem != null && metaItem.Item != null)
         {
            List<AutoListColumnHeaderItem> columnCollection = new List<AutoListColumnHeaderItem>();

            if (this.SelectedItems != null)
            {
               // create a checkbox, radio button or none depends on SelectionMode
               switch (this.SelectionMode)
               {
                  case AutoListSelectionMode.None:
                     break;
                  // checkbox
                  case AutoListSelectionMode.Multiple:
                     columnCollection.Add(new AutoListColumnHeaderItem(
                        new AutoListGridViewColumn("__AutoListSelector")
                        {
                           IsSelectorColumn = true,
                           IsResizable = false,
                           CellTemplate = this.CreateSelectorControl<CheckBox>("IsSelected", false)
                        },
                        new AutoListHeaderAttribute(string.Empty, 0, this.uniqueListIdentifier)
                        {
                           IsVisibleInContextMenu = false
                        }));
                     break;
                  // radio button
                  case AutoListSelectionMode.Single:
                     columnCollection.Add(new AutoListColumnHeaderItem(
                        new AutoListGridViewColumn("__AutoListSelector")
                        {
                           IsSelectorColumn = true,
                           IsResizable = false,
                           CellTemplate = this.CreateSelectorControl<RadioButton>("IsSelected", false)
                        },
                        new AutoListHeaderAttribute(string.Empty, 0, this.uniqueListIdentifier)
                        {
                           IsVisibleInContextMenu = false
                        }));
                     break;
               }
            }

            columnCollection.AddRange(this.CreateHeadersFromMetaItem(metaItem));

            // reorder items
            columnCollection = columnCollection.OrderBy(c => c.Attribute.Order).ToList();

            // create post generated collection
            AutoListColumnConfiguration columnConfiguration = new AutoListColumnConfiguration();
            columnConfiguration.ColumnChanged += OnColumnConfigurationColumnChanged;

            // only if there is any column except selector column
            if (columnCollection.Count > 1)
            {
               this.internalGridView.Columns.Clear();

               // add to the gridview, unfortunately GridViewColumnCollection is an observablecollection which does not support addrange and throws an exception if you implement it as external method due NotifyCollectionChanged
               var contextItems = new List<MenuItem>();

               // add excel export at the top
               var exportMenuItem = new MenuItem { IsCheckable = false, Header = "Export..." };
               exportMenuItem.Click += OnExcelExportClick;
               exportMenuItem.Tag = 0;
               exportMenuItem.Visibility = this.AllowExcelExport ? Visibility.Visible : System.Windows.Visibility.Collapsed;
               contextItems.Add(exportMenuItem);              

               columnCollection.ForEach(c =>
                  {
                     // add to the columncollection
                     this.internalGridView.Columns.Add(c.Column);

                     // create context menu for this column
                     if (c.Column.IsSelectorColumn == false)
                     {
                        if (c.Attribute.IsVisibleInContextMenu)
                        {
                           var menuItem = new AutoListContextMenuItem(c.Column.UniqueColumnIdentifier) { IsCheckable = true, IsChecked = c.Attribute.IsVisibleByDefault, Header = c.Attribute.Header };
                           menuItem.Click += OnContextMenuItemClick;
                           menuItem.Tag = 1;
                           contextItems.Add(menuItem);
                        }

                        // fill the generated collection
                        columnConfiguration.Add(new AutoListGeneratedColumn(c.Column.UniqueColumnIdentifier, c.Column.Header as string, c.Column.IsResizable, c.Column.IsVisible));
                     }
                  });

               // sort it alphabeticaly
               this.internalHeaderContextMenu.ItemsSource = contextItems.OrderBy(m => m.Tag).ThenBy(m => m.Header);               

               this.ToggleContextMenuItems();
               this.ToggleExcelExportMenu();

               this.isGenerated = true;

               // if there is any binding then execute the action
               if (this.OnGenerated != null)
               {
                  this.OnGenerated.Invoke(columnConfiguration);
               }
            }
            else
            {
               throw new ArgumentNullException(string.Format("Could not find AutoListHeaderAttribute on any property of {0}.", metaItem.Item.GetType().Name));
            }
         }
      }

      private Binding CreateBindingForColumn(PropertyInfo propertyInfo, AutoListHeaderAttribute attribute)
      {
         var binding = new Binding("Item." + propertyInfo.Name) { Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
         binding.TargetNullValue = attribute.NullValue;

         if (string.IsNullOrWhiteSpace(attribute.StringFormat) == false)
         {
            binding.StringFormat = attribute.StringFormat;
         }

         if (propertyInfo.PropertyType.IsEnum)
         {
            binding.Converter = new TranslatorConverter(propertyInfo.PropertyType, this.uniqueListIdentifier);
         }

         return binding;
      }

      // maybe not the best to use "magic" strings to identify properties but better then execute the same code all the time
      private void OnColumnConfigurationColumnChanged(object sender, AutoListGeneratedColumnChangedEventArgs e)
      {
         // get the column
         var column = this.internalGridView.Columns.OfType<AutoListGridViewColumn>().Single(c => c.UniqueColumnIdentifier == e.UniqueColumnIdentifier);
         column.GetType().GetProperty(e.PropertyName).SetValue(column, e.NewValue, null);

         if (e.PropertyName == "Header")
         {
            var index = this.internalGridView.Columns.IndexOf(column);
            // gridview does not support refresh, must be removed and readded
            this.internalGridView.Columns.Remove(column);
            this.internalGridView.Columns.Insert(index, column);
         }

         if (e.PropertyName == "IsVisible")
         {
            // keep context menu synced
            var contextMenuItem = this.internalHeaderContextMenu.ItemsSource.OfType<AutoListContextMenuItem>().First(m => m.UniqueColumnIdentifier == e.UniqueColumnIdentifier);
            contextMenuItem.IsChecked = e.NewValue;
         }
      }

      // hide / unhide column
      private void OnContextMenuItemClick(object sender, RoutedEventArgs e)
      {
         var menuItem = (sender as AutoListContextMenuItem);
         if (menuItem != null)
         {
            var column = this.internalGridView.Columns.OfType<AutoListGridViewColumn>().SingleOrDefault(c => c.UniqueColumnIdentifier == menuItem.UniqueColumnIdentifier);

            if (column != null)
            {
               column.IsVisible = menuItem.IsChecked;
            }
         }
      }

      // on item double click, execute action [if there is any]
      private void OnListItemDoubleClicked(object sender, MouseButtonEventArgs e)
      {
         var item = ((FrameworkElement)e.OriginalSource).DataContext as AutoListItem<object>;
         if (item != null && this.OnItemDoubleClick != null && e.ChangedButton == MouseButton.Left)
         {
            this.OnItemDoubleClick.Invoke(item.Item);
         }
      }

      private void StretchGridViewColumns(double width)
      {
         var columns = this.internalGridView.Columns.OfType<AutoListGridViewColumn>();
         var sumColumnWidth = columns.Sum(c => c.ActualWidth);

         // do not resize before ActualWidth
         if (sumColumnWidth > 0.0 && columns.Any(c => c.ActualWidth == 0.0 && c.IsVisible) == false)
         {
            var affectedColumns = columns.Where(c => c.IsVisible && c.IsResizable);

            // resize every column a little bit so determine the difference
            // 13 is a magic number :) without it scrollbar becomes visible
            var plus = (width - sumColumnWidth - 13) / affectedColumns.Count();

            // resize only visible and resizeable columns 
            foreach (var column in affectedColumns)
            {
               // store min. req. width
               if (double.IsNaN(column.DesiredWidth))
               {
                  column.DesiredWidth = column.ActualWidth;
               }

               // resize column
               column.Width = Math.Max(column.ActualWidth + plus, column.DesiredWidth);
            }
         }
      }

      private void OnExcelExportClick(object sender, RoutedEventArgs e)
      {
         var saveDialog = new SaveFileDialog();
         saveDialog.DefaultExt = ".xlsx";
         saveDialog.Filter = "Excel|*.xlsx|All Files|*.*";
         saveDialog.Title = "Export to Excel";

         string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);     
         saveDialog.InitialDirectory = path;

         if (saveDialog.ShowDialog() == true)
         {
            this.ExportToExcel(saveDialog.FileName);
         }
      }

      private void ExportToExcel(string fileName)
      {
         this.IsBusy = true;

         var excelWorker = new BackgroundWorker();
         excelWorker.RunWorkerCompleted += (s, e) =>
            {
               this.IsBusy = false;
            };

         // get visible rows
         var visibleItems = CollectionViewSource.GetDefaultView(this.internalListView.ItemsSource)
            .OfType<AutoListItem<object>>()
            .Select(i => i.Item).ToList();

         // get visible columns in the current order
         var visibleColumns = this.internalGridView.Columns
            .OfType<AutoListGridViewColumn>()
            .Where(c => c.IsVisible && c.IsSelectorColumn == false)
            .Select(h => h.Header == null ? string.Empty : h.Header.ToString())
            .ToList();

         excelWorker.DoWork += (s, e) =>
            {                              
               // final collection
               var rows = new List<ExcelCell[]>();

               // transform items into excel compatible
               foreach (var item in visibleItems)
               {
                  var properties = item.GetType().GetProperties();
                  rows.Add(new ExcelCell[visibleColumns.Count]);

                  // find properties on list item type
                  foreach (var p in properties)
                  {
                     var attributes = p.GetCustomAttributes(typeof(AutoListHeaderAttribute), false) as AutoListHeaderAttribute[];
                     if (attributes != null)
                     {
                        // check list identifier
                        var attribute = attributes.FirstOrDefault(a => a.UniqueListIdentifier == this.uniqueListIdentifier);

                        // only visible columns
                        if (attribute != null && visibleColumns.Contains(attribute.Header))
                        {
                           var index = visibleColumns.IndexOf(attribute.Header);
                           var cell = new ExcelCell();
                           cell.Index = index;
                           cell.Content = p.GetValue(item, null) ?? attribute.NullValue;

                           rows.Last()[index] = cell;
                        }
                     }
                  }
               }

               var excelExporter = new ExcelExporter(rows, visibleColumns);
               excelExporter.Save(fileName);
            };

         excelWorker.RunWorkerAsync();
      }
   }
}
