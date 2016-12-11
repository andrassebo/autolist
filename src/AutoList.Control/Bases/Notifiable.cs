namespace AutoList.Control.Bases
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;
   using System.ComponentModel;
   using System.Linq.Expressions;

   public abstract class Notifiable : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;

      private void RaisePropertyChanged(string propertyName)
      {
         var handler = this.PropertyChanged;
         if (handler != null)
         {
            handler(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      protected void RaisePropertyChanged<TProperty>(Expression<Func<TProperty>> propertyExpression)
      {
         var propertyName = this.ExtractPropertyName(propertyExpression);

         this.RaisePropertyChanged(propertyName);
      }

      protected bool SetAndRaise<TProperty>(Expression<Func<TProperty>> propertyExpression, ref TProperty backingField, TProperty value)         
      {
         bool isChanged = false;

         if (EqualityComparer<TProperty>.Default.Equals(backingField, value) == false)
         {
            var oldValue = backingField;

            backingField = value;

            var propertyName = this.ExtractPropertyName(propertyExpression);

            this.RaisePropertyChanged(propertyName);

            this.OnPropertyChanged(propertyName, oldValue, value);

            isChanged = true;
         }

         return isChanged;
      }

      protected virtual void OnPropertyChanged<TProperty>(string propertyName, TProperty oldValue, TProperty newValue)
      {          
      }

      private string ExtractPropertyName<TProperty>(Expression<Func<TProperty>> propertyExpression)
      {         
         MemberExpression me = propertyExpression.Body as MemberExpression;
         if (me == null)
         {
            UnaryExpression ue = propertyExpression.Body as UnaryExpression;
            if (ue != null)
            {
               me = ue.Operand as MemberExpression;
            }
         }

         if ((me != null) && string.IsNullOrEmpty(me.Member.Name) == false)
         {
            return me.Member.Name;
         }

         throw new ArgumentException("Property could not be found.");
      }
   }
}
