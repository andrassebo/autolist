namespace AutoList.Client.ViewModels
{
   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Linq;
   using System.Linq.Expressions;
   using System.Text;

   using System.Collections.ObjectModel;
   using System.Threading;
   using System.Windows;

   using AutoList.Control.Attributes;
   using AutoList.Client.Items;
   using AutoList.Client.Bases;
   using AutoList.Control.Configurations;
   using AutoList.Base.Extensions;
   using AutoList.Excel;   

   public class MainWindowViewModel: ViewModelBase
   {
      private readonly BackgroundWorker backgroundListWorker = new BackgroundWorker();

      [AutoListItemsSource("Main")]
      public ObservableCollection<ListItem> Items { get; private set; }

      [AutoListItemsSource("Selected")]
      public ObservableCollection<ListItem> SelectedItems { get; private set; }

      public Dictionary<string, Expression<Func<object, object>>> GroupByProperties { get; private set; } 

      public RelayCommand AddItemsCommand { get; private set; }

      public RelayCommand ChangeHeaderCommand { get; private set; }

      public RelayCommand ToggleSelectionCommand { get; private set; }

      public Action<AutoListColumnConfiguration> Generated { get; private set; }

      public Predicate<object> FilterPredicate { get; private set; }      

      // must be an object and not ListItem, otherwise won't work
      public Action<object> OnItemDoubleClicked { get; private set; }      

      private AutoListColumnConfiguration generated;

      public MainWindowViewModel()
      {         
         this.Items = new ObservableCollection<ListItem>();
         this.SelectedItems = new ObservableCollection<ListItem>();
         backgroundListWorker.DoWork += backgroundListWorker_DoWork;
         backgroundListWorker.RunWorkerCompleted += (s, e) =>
            {
               this.RaisePropertyChanged("Items");
            };

         this.AddItemsCommand = new RelayCommand(() => 
         {
            backgroundListWorker.RunWorkerAsync();
         });

         this.ChangeHeaderCommand = new RelayCommand(() =>
            {
               foreach (var c in generated.GetColumns(c => true))
               {
                  c.Header = c.Header + " changed";
               }               
            });

         this.ToggleSelectionCommand = new RelayCommand(() =>
            {
               
            });

         this.Generated = new Action<AutoListColumnConfiguration>(OnAutoListConfigured);
         this.OnItemDoubleClicked = new Action<object>(OnListItemDoubleClicked);

         this.GroupByProperties = TypeExtensions.GetPropertySelectorsWithName<ListItem>(p => p.GetCustomAttributes(typeof(AutoListHeaderAttribute), false).Any());
         this.GroupByProperties.Add("none", null);
      }

      private void backgroundListWorker_DoWork(object sender, DoWorkEventArgs e)
      {
         this.DoWork();
      }
      
      private void OnAutoListConfigured(AutoListColumnConfiguration e)
      {
         this.generated = e;
      }

      private void OnListItemDoubleClicked(object item)
      {         
         MessageBox.Show("Clicked on " + (item as ListItem).Name, "AutoList DoubleClick");
      }

      private void DoWork()
      {
         this.IsBusy = true;

         var items = new ObservableCollection<ListItem>();

         // demonstrate some slow input, e.g: WCF or EF to show busy indicator
         items.Add(new ListItem { Name = "John Redneck", Address = "Texas", PhoneNumber = "1-800-2391234123", Gender = Enums.Gender.Male, DateOfBirth = new DateTime(1984, 11, 10) });
         items.Add(new ListItem { Name = "Mary Dallas", Address = "Seattle", PhoneNumber = "1-800-4832748234", Gender = Enums.Gender.Female, DateOfBirth = new DateTime(1988, 1, 23), Progress = 12 });
         items.Add(new ListItem { Name = "Mike Dawn", Address = "NYC", PhoneNumber = "1-800-44467777", Gender = Enums.Gender.Male });
         items.Add(new ListItem { Name = "Bob Miles", Address = "Washington", PhoneNumber = "1-800-8884444", Gender = Enums.Gender.Male, DateOfBirth = new DateTime(1990, 3, 23) });
         items.Add(new ListItem { Name = "Jennifer Bix", Address = "Austin", PhoneNumber = "1-800-32323222", Gender = Enums.Gender.Female, IsActive = true });

         //for (int i = 0; i < 1000; i++)
         //{
         //   this.Items.Add(new ListItem { Name = "Sample Name #" + i, Address = "Sample Address #" + i, Gender = (i % 2 == 0) ? Enums.Gender.Male : Enums.Gender.Female });
         //}

         // simulate wait
         Thread.Sleep(2000);

         this.Items = items;

         this.IsBusy = false;
      }

      private bool _IsBusy = false;
      public bool IsBusy
      {
         get
         {
            return this._IsBusy;
         }
         set
         {
            if (this._IsBusy != value)
            {
               this._IsBusy = value;
               this.RaisePropertyChanged("IsBusy");
            }
         }
      }

      private ListItem _FocusedItem = null;
      public ListItem FocusedItem
      {
         get
         {
            return this._FocusedItem;
         }
         set
         {
            if (this._FocusedItem != value)
            {
               this._FocusedItem = value;
               this.RaisePropertyChanged("FocusedItem");
            }
         }
      }

      private string _FilterText = string.Empty;
      public string FilterText
      {
         get
         {
            return this._FilterText;
         }
         set
         {
            if (this._FilterText != value)
            {
               this._FilterText = value;
               this.RaisePropertyChanged("FilterText");

               if (string.IsNullOrWhiteSpace(value) == false)
               {
                  this.FilterPredicate = i => (i as ListItem).Name.ToLower().StartsWith(value);
               }
               else
               {
                  this.FilterPredicate = i => true;
               }
               this.RaisePropertyChanged("FilterPredicate");               
            }
         }
      }
     
      private Expression<Func<object, object>> _GroupByPredicate = null;
      public Expression<Func<object, object>> GroupByPredicate
      {
         get
         {
            return this._GroupByPredicate;
         }
         set
         {
            if (this._GroupByPredicate != value)
            {
               this._GroupByPredicate = value;
               this.RaisePropertyChanged("GroupByPredicate");
            }
         }
      }      
   }
}