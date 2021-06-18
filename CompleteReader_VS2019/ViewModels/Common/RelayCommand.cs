using System;
using System.Windows.Input;

namespace CompleteReader.ViewModels.Common
{
    public class RelayCommand : ICommand
    {
        // Event that fires when the enabled/disabled state of the cmd changes
        public event EventHandler CanExecuteChanged;

        // Delegate for method to call when the cmd needs to be executed
        protected Action<object> _execute;

        // Delegate for method that determines if cmd is enabled/disabled
        protected Predicate<object> _canExecute;

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        // Call the delegate if it's not null
        public void Execute(object parameter) => _execute?.Invoke(parameter);

        public RelayCommand(Action<object> executeMethod = null, Predicate<object> canExecuteMethod = null)
        {
            _execute = executeMethod;
            _canExecute = canExecuteMethod;
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T> executeAction) : this(executeAction, null)
        {
        }

        public RelayCommand(Action<T> executeAction, Predicate<object> canExecute)
        {
            _execute = executeAction;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute((T)parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
