using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CompleteReader.ViewModels.Common.Events
{
    public static class ItemClickCommand
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(ItemClickCommand),
            new PropertyMetadata(null, OnCommandPropertyChanged));

        public static void SetCommand(DependencyObject d, ICommand value) => d.SetValue(CommandProperty, value);

        public static ICommand GetCommand(DependencyObject d) => (ICommand)d.GetValue(CommandProperty);

        private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListViewBase control)
            {
                if (e.NewValue != null)
                {
                    control.ItemClick += OnItemClick;
                }
                else
                {
                    control.ItemClick -= OnItemClick;
                }
            }
        }

        private static void OnItemClick(object sender, ItemClickEventArgs e)
        {
            ListViewBase control = sender as ListViewBase;
            var command = GetCommand(control);

            if (command?.CanExecute(e) == true)
                command.Execute(e);
        }
    }
}
