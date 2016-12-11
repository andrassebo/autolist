namespace AutoList.Client
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;
   using System.Windows.Input;

   // simple implementation of ICommand, without canExecuteChanged
   public class RelayCommand : ICommand
   {
      public Action Command { get; private set; }

      public RelayCommand(Action command)
      {
         this.Command = command;
      }

      public bool CanExecute(object parameter)
      {
         return true;
      }

      public event EventHandler CanExecuteChanged;

      public void Execute(object parameter)
      {
         this.Command.Invoke();
      }
   }
}
