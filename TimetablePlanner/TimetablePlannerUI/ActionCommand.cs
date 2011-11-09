using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TimetablePlannerUI
{
    public class ActionCommand : ICommand
    {
        private readonly Action _execute = null;
        private readonly Predicate<object> _canExecute = null;
        public event EventHandler CanExecuteChanged;

        public ActionCommand(Action execute) : this(execute, null) { }

        public ActionCommand(Action execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute != null ? _canExecute(parameter) : true;
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
                _execute();
        }

        public void OnCanExecuteChanged()
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
