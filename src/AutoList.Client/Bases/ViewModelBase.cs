using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AutoList.Client.Bases
{
   // really simple implementation of INotifyPropertyChanged
   public abstract class ViewModelBase : INotifyPropertyChanged
   {
      protected void RaisePropertyChanged(string propertyName)
      {
         var handler = this.PropertyChanged;
         if (handler != null)
         {
            handler(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;
   }
}
