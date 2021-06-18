using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace PDFNetUniversalSamples.Common
{
    public class NavigationService
    {
        public void Navigate(Type sourcePageType)
        {
            ((Frame)Window.Current.Content).Navigate(sourcePageType);
        }
        public void Navigate(Type sourcePageType, object parameter)
        {
            ((Frame)Window.Current.Content).Navigate(sourcePageType, parameter);
        }
        public bool CanGoBack()
        {
            return ((Frame)Window.Current.Content).CanGoBack;
        }
        public void GoBack()
        {
            ((Frame)Window.Current.Content).GoBack();
        }
    }
}
