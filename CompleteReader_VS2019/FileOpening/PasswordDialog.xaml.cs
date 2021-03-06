using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CompleteReader.FileOpening
{
    public sealed partial class PasswordDialog : UserControl
    {
        public PasswordDialog()
        {
            this.InitializeComponent();
            this.DataContextChanged += PasswordDialog_DataContextChanged;

            if (Utilities.UtilityFunctions.GetDeviceFormFactorType() == Utilities.UtilityFunctions.DeviceFormFactorType.Phone)
            {
                InnerGrid.Margin = new Thickness(10, 30, 10, 20);
            }
        }

        void PasswordDialog_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                PasswordEntry.Focus(FocusState.Programmatic);
            }
        }
    }
}
