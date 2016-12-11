namespace AutoList.Client.Items
{
   using System;

   using AutoList.Client.Bases;
   using AutoList.Client.Enums;
   using AutoList.Control.Attributes;
   using AutoList.Control.Enums;
   using System.Windows;

   public class ListItem : ViewModelBase
   {
      [AutoListHeader("Name", 10, "Main", HorizontalConentAlignment=HorizontalAlignment.Left)]
      [AutoListHeader("Name", 10, "Selected")]
      public string Name { get; set; }

      [AutoListHeader("Current address", 50, "Main", IsResizable = false, Width=200)]
      [AutoListHeader("Address", 20, "Selected")]
      public string Address { get; set; }


      private string _phoneNumber = string.Empty;
      [AutoListHeader("Phone number", 30, "Main", IsVisibleByDefault = false)]
      [AutoListHeader("Phone", 30, "Selected")]
      public string PhoneNumber
      {
         get
         {
            return this._phoneNumber;
         }
         set
         {
            this._phoneNumber = value;
            this.RaisePropertyChanged("PhoneNumber");
         }
      }

      [AutoListHeader("Age", 40, "Main")]
      [AutoListHeader("Age", 40, "Selected")]
      public int? Age
      {
         get
         {
            return DateOfBirth.HasValue ? DateTime.Now.Subtract(DateOfBirth.Value).Days / 365 as int? : null;
         }
      }

      [AutoListHeader("Born", 45, "Main", StringFormat="d", NullValue="n/a")]
      public DateTime? DateOfBirth { get; set; }

      public DateTime LastSeen { get; set; }

      public string MotherName { get; set; }

      [AutoListHeader("Gender", 60, "Main")]
      public Gender Gender { get; set; }

      [AutoListHeader("Active?", 70, "Main", ColumnType=AutoListColumnType.Checkbox)]
      public bool IsActive { get; set; }
      
      private int _progress = 0;
      [AutoListHeader("Progress", 100, "Main", ColumnType=AutoListColumnType.ProgressBar)]
      public int Progress
      {
         get
         {
            return _progress;
         }
         set
         {
            this._progress = value;
            this.RaisePropertyChanged("Progress");
         }
      }

      //[AutoListHeaderAttribute("Column1", 100, "Main")]
      //public string Column1 { get; set; }

      //[AutoListHeaderAttribute("Column2", 100, "Main")]
      //public string Column2 { get; set; }

      //[AutoListHeaderAttribute("Column3", 100, "Main")]
      //public string Column3 { get; set; }

      //[AutoListHeaderAttribute("Column4", 100, "Main")]
      //public string Column4 { get; set; }

      //[AutoListHeaderAttribute("Column5", 100, "Main")]
      //public string Column5 { get; set; }

      //[AutoListHeaderAttribute("Column6", 100, "Main")]
      //public string Column6 { get; set; }

      //[AutoListHeaderAttribute("Column7", 100, "Main")]
      //public string Column7 { get; set; }

      //[AutoListHeaderAttribute("Column8", 100, "Main")]
      //public string Column8 { get; set; }

      //[AutoListHeaderAttribute("Column9", 100, "Main")]
      //public string Column9 { get; set; }

      //[AutoListHeaderAttribute("Column10", 100, "Main")]
      //public string Column10 { get; set; }

      //[AutoListHeaderAttribute("Column11", 100, "Main")]
      //public string Column11 { get; set; }

      //[AutoListHeaderAttribute("Column12", 100, "Main")]
      //public string Column12 { get; set; }

      //[AutoListHeaderAttribute("Column13", 100, "Main")]
      //public string Column13 { get; set; }

      //[AutoListHeaderAttribute("Column14", 100, "Main")]
      //public string Column14 { get; set; }

      //[AutoListHeaderAttribute("Column15", 100, "Main")]
      //public string Column15 { get; set; }

      //[AutoListHeaderAttribute("Column16", 100, "Main")]
      //public string Column16 { get; set; }

      //[AutoListHeaderAttribute("Column17", 100, "Main")]
      //public string Column17 { get; set; }

      //[AutoListHeaderAttribute("Column18", 100, "Main")]
      //public string Column18 { get; set; }

      //[AutoListHeaderAttribute("Column19", 100, "Main")]
      //public string Column19 { get; set; }

      //[AutoListHeaderAttribute("Column20", 100, "Main")]
      //public string Column20 { get; set; }
      //[AutoListHeaderAttribute("Column21", 100, "Main")]
      //public string Column21 { get; set; }

      //[AutoListHeaderAttribute("Column22", 100, "Main")]
      //public string Column22 { get; set; }

      //[AutoListHeaderAttribute("Column23", 100, "Main")]
      //public string Column23 { get; set; }

      //[AutoListHeaderAttribute("Column24", 100, "Main")]
      //public string Column24 { get; set; }

      //[AutoListHeaderAttribute("Column25", 100, "Main")]
      //public string Column25 { get; set; }

      //[AutoListHeaderAttribute("Column26", 100, "Main")]
      //public string Column26 { get; set; }

      //[AutoListHeaderAttribute("Column27", 100, "Main")]
      //public string Column27 { get; set; }

      //[AutoListHeaderAttribute("Column28", 100, "Main")]
      //public string Column28 { get; set; }

      //[AutoListHeaderAttribute("Column29", 100, "Main")]
      //public string Column29 { get; set; }

      //[AutoListHeaderAttribute("Column30", 100, "Main")]
      //public string Column30 { get; set; }
   }
}
