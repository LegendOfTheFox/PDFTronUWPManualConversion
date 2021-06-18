using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace PDFViewCtrlDemo_Windows10.ViewModels.Common
{
    public class RelayCommand : ICommand
    {
        // Event that fires when the enabled/disabled state of the cmd changes
        public event EventHandler CanExecuteChanged;

        // Delegate for method to call when the cmd needs to be executed        
        private readonly Action<object> _targetExecuteMethod;

        // Delegate for method that determines if cmd is enabled/disabled        
        private readonly Predicate<object> _targetCanExecuteMethod;

        public bool CanExecute(object parameter)
        {
            return _targetCanExecuteMethod == null || _targetCanExecuteMethod(parameter);
        }

        public void Execute(object parameter)
        {
            // Call the delegate if it's not null
            if (_targetExecuteMethod != null) _targetExecuteMethod(parameter);
        }

        public RelayCommand(Action<object> executeMethod, Predicate<object> canExecuteMethod = null)
        {
            _targetExecuteMethod = executeMethod;
            _targetCanExecuteMethod = canExecuteMethod;
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null) CanExecuteChanged(this, EventArgs.Empty);
        }
    }

    public static class PointerPressedCommand
    {
        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached("Command", typeof(ICommand),
        typeof(PointerPressedCommand), new PropertyMetadata(null, OnCommandPropertyChanged));

        public static void SetCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(CommandProperty);
        }

        private static void OnCommandPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = d as FrameworkElement;
            if (control != null)
                control.PointerPressed += Control_PointerPressed;
        }

        static void Control_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            var command = GetCommand(control);

            if (command != null && command.CanExecute(e.Pointer))
            {
                command.Execute(new Tuple<object, PointerRoutedEventArgs>(sender, e));

            }
        }
    }

    public static class TextChangedCommand
    {
        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached("Command", typeof(ICommand),
        typeof(TextChangedCommand), new PropertyMetadata(null, OnCommandPropertyChanged));

        public static void SetCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(CommandProperty);
        }

        private static void OnCommandPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = d as TextBox;
            if (control != null)
                control.TextChanged += control_TextChanged;
        }

        private static void control_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox control = sender as TextBox;
            var command = GetCommand(control);

            if (command != null && command.CanExecute(e))
            {
                command.Execute(e);
            }
        }
    }

    public static class PasswordChangedCommand
    {
        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached("Command", typeof(ICommand),
        typeof(PasswordChangedCommand), new PropertyMetadata(null, OnCommandPropertyChanged));

        public static void SetCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(CommandProperty);
        }

        private static void OnCommandPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = d as PasswordBox;
            if (control != null)
                control.PasswordChanged += control_PasswordChanged;
        }

        private static void control_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox control = sender as PasswordBox;
            var command = GetCommand(control);

            if (command != null && command.CanExecute(control.Password))
            {
                command.Execute(control.Password);
            }
        }
    }

    public static class KeyUpCommand
    {
        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached("Command", typeof(ICommand),
        typeof(KeyUpCommand), new PropertyMetadata(null, OnCommandPropertyChanged));

        public static void SetCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(CommandProperty);
        }

        private static void OnCommandPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = d as FrameworkElement;
            if (control != null)
                control.KeyUp += control_KeyUp;
        }

        static void control_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            var command = GetCommand(control);

            if (command != null && command.CanExecute(e))
            {
                command.Execute(e);
            }
        }
    }
}
