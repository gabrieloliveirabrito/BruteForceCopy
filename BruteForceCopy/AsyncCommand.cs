using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;

namespace BruteForceCopy
{
    public class AsyncCommand : AsyncCommand<object>
    {
        public AsyncCommand(Func<object, Task> action, Func<object, bool> canExecute = null) : base(action, canExecute)
        {

        }
    }

    public class AsyncCommand<T> : ICommand
    {
        private Func<T, Task> action;
        private Func<T, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncCommand(Func<T, Task> action, Func<T, bool> canExecute = null)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute((T)parameter);
        }

        public async void Execute(object parameter)
        {
            if (CanExecute(parameter))
                await action((T)parameter);
        }
    }
}