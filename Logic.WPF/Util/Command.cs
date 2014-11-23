﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Logic.Util
{
    public class Command : ICommand
    {
        private Action<object> _execute;
        private Func<object, bool> _canExecute;

        public Command(Action<object> execute, Func<object, bool> canExecute)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
            {
                return _canExecute(parameter);
            }
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (_execute != null)
            {
                _execute(parameter);
            }
        }
    }
}
