using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PDFViewCtrlDemo_Windows10.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewerPage : Page
    {
        public ViewerPage()
        {
            this.InitializeComponent();

            ViewModels.ViewerPageViewModel viewModel = new ViewModels.ViewerPageViewModel();
            this.DataContext = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;

            bool isHardwareButtonsAPIPresent = 
                Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");

            if (isHardwareButtonsAPIPresent)
            {
                OutlineBorder.HorizontalAlignment = HorizontalAlignment.Center;
                AnnotationListBorder.HorizontalAlignment = HorizontalAlignment.Center;
                UserBookmarksBorder.HorizontalAlignment = HorizontalAlignment.Center;
                GoBackButton.Visibility = Visibility.Collapsed;
            }

            this.SizeChanged += ViewerPage_SizeChanged;
            MagnifyingHost.SizeChanged += MagnifyingHost_SizeChanged;
        }

        private void MagnifyingHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((this.DataContext as ViewModels.ViewerPageViewModel).MagnifyingControl != null)
            {
                (this.DataContext as ViewModels.ViewerPageViewModel).MagnifyingControl.ViewModel.EffectiveHeight = e.NewSize.Height * 0.4;
            }
            if ((DataContext as ViewModels.ViewerPageViewModel).IsMagnifying)
            {
                CalculateMagnifyingToolSize(e.NewSize);
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("ShowFindText", StringComparison.OrdinalIgnoreCase))
            {
                if ((DataContext as ViewModels.ViewerPageViewModel).ShowFindText)
                {
                    SearchTextBox.Focus(FocusState.Programmatic);
                }
            }
            else if (e.PropertyName.Equals("IsAppBarVisible", StringComparison.OrdinalIgnoreCase))
            {
                if ((DataContext as ViewModels.ViewerPageViewModel).IsAppBarVisible)
                {
                    ViewerCommandBad.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                }
                else
                {
                    ViewerCommandBad.ClosedDisplayMode = AppBarClosedDisplayMode.Hidden;
                    ViewerCommandBad.IsOpen = false;
                }
            }
            else if (e.PropertyName.Equals("IsMagnifying", StringComparison.OrdinalIgnoreCase))
            {
                CalculateMagnifyingToolSize(new Size(MagnifyingHost.ActualWidth, MagnifyingHost.ActualHeight));
                ViewModels.ViewerPageViewModel vm = DataContext as ViewModels.ViewerPageViewModel;
                if (vm != null && vm.MagnifyingControl != null)
                {
                    vm.MagnifyingControl.ViewModel.MagnifiedToolManager.PopupLimitingTarget = MagnifyingHost;
                }
            }
        }

        private void ViewerPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newWidth = e.NewSize.Width;

            OutlineBorder.Width = Math.Min(OutlineBorder.MaxWidth, newWidth);
            AnnotationListBorder.Width = Math.Min(AnnotationListBorder.MaxWidth, newWidth);
            UserBookmarksBorder.Width = Math.Min(UserBookmarksBorder.MaxWidth, newWidth);
        }

        private void CalculateMagnifyingToolSize(Size size)
        {
            MagnifyingHost.MagnifiedHeight = MagnifyingHost.MaxMagnifiedHeightRatio * size.Height;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            (this.DataContext as ViewModels.ViewerPageViewModel).Activate();

            if (e.Parameter != null && e.Parameter is pdftron.PDF.PDFDoc)
            {
                (this.DataContext as ViewModels.ViewerPageViewModel).OpenDoc(e.Parameter as pdftron.PDF.PDFDoc);
            }
            else if (e.Parameter != null && e.Parameter is string)
            {
                (this.DataContext as ViewModels.ViewerPageViewModel).OpenURL(e.Parameter as string);
            }
            else if (e.Parameter != null && e.Parameter is pdftron.PDF.DocumentConversion)
            {
                pdftron.PDF.DocumentConversion conversion = e.Parameter as pdftron.PDF.DocumentConversion;
                (this.DataContext as ViewModels.ViewerPageViewModel).Open(conversion);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            (this.DataContext as ViewModels.ViewerPageViewModel).Deactivate();
        }

        private void SearchTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e != null)
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    FocusStealer.IsTabStop = true;
                    FocusStealer.Focus(FocusState.Programmatic);
                    FocusStealer.IsTabStop = false;

                    ViewModels.FindTextViewModel viewModel = SearchTextBox.DataContext as ViewModels.FindTextViewModel;
                    if (viewModel != null)
                    {
                        viewModel.SearchTerm = SearchTextBox.Text;
                        viewModel.FindCommand.Execute(SearchTextBox);
                    }
                }
            }
        }
    }
}
