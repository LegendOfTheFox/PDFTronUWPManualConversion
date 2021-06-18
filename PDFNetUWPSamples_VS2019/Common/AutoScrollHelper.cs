using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PDFNetUniversalSamples.Common
{
    public static class AutoScrollHelper
    {
        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }

        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollHelper), new PropertyMetadata(false, AutoScrollPropertyChanged));

        private static async void AutoScrollPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as ScrollViewer;

            if (scrollViewer != null && (bool)e.NewValue)
            {
                // this delay is needed on the phone to make it able to scroll if text has just been added and the layout hasn't had time to update
                await System.Threading.Tasks.Task.Delay(200); 

                scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
            }
        }
    }
}
