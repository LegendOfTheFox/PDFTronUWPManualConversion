﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PDFNetUniversalSamples.Common
{
    public static class ItemClickCommand
    {
        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached("Command", typeof(ICommand),
        typeof(ItemClickCommand), new PropertyMetadata(null, OnCommandPropertyChanged));

        public static void SetCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(CommandProperty);
        }

        private static Dictionary<ListViewBase, ItemClickEventHandler> _BoundControls = new Dictionary<ListViewBase,ItemClickEventHandler>();
        private static void OnCommandPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = d as ListViewBase;
            if (control == null)
            {
                return;
            }

            if (_BoundControls.ContainsKey(control))
            {
                control.ItemClick -= _BoundControls[control];
            }

            ItemClickEventHandler itemClickHandler = new ItemClickEventHandler(OnItemClick);
            control.ItemClick += itemClickHandler;
            _BoundControls[control] = itemClickHandler;
        }

        private static void OnItemClick(object sender, ItemClickEventArgs e)
        {
            var control = sender as ListViewBase;
            var command = GetCommand(control);

            if (command != null && command.CanExecute(e.ClickedItem))
            {
                command.Execute(e.ClickedItem);
            }
        }
    }
}
