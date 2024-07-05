﻿using Calc.Core.Objects.Buildups;
using Calc.Core.Objects.Materials;
using Calc.MVVM.Helpers;
using Calc.MVVM.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Calc.MVVM.Views
{
    public partial class BuildupSelectionView : Window
    {
        private double originalLeft;
        private readonly BuildupSelectionViewModel BuildupSelectionVM;

        public BuildupSelectionView(BuildupSelectionViewModel buildupSelectionVM)
        {
            this.BuildupSelectionVM = buildupSelectionVM;
            this.DataContext = BuildupSelectionVM;
            InitializeComponent();
        }

        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var currentText = (sender as TextBox).Text;
            BuildupSelectionVM.HandleSearchTextChanged(currentText);
        }

        private void ListViewSelected(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListView).ContainerFromElement(e.OriginalSource as DependencyObject) as ListViewItem;
            if (item != null && item.IsSelected)
            {
                item.IsSelected = false;
                e.Handled = true;
            }
        }

        private void ListViewLoaded(object sender, RoutedEventArgs e)
        {
            var listView = sender as ListView;
            if(listView.SelectedItem!=null)
            {
                listView.ScrollIntoView(listView.SelectedItem);
            }
        }

        private void ListViewItemBuildupDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Enables scrolling item by item in the list view
        /// </summary>
        private void ListViewPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer viewer = ViewHelper.FindVisualChild<ScrollViewer>(sender as ListView);

            if (e.Delta > 0)
            {
                viewer.LineUp();
            }
            if (e.Delta < 0)
            {
                viewer.LineDown();
            }
            e.Handled = true;
        }

        private void SourceCheckChanged(object sender, RoutedEventArgs e)
        {
            BuildupSelectionVM.HandleSourceCheckChanged();
        }


        private void WindowLocationChanged(object sender, RoutedEventArgs e)
        {
            originalLeft = this.Left;
        }

        private void WindowLocationChanged(object sender, EventArgs e)
        {
            originalLeft = this.Left;
        }

        private async void BuildupSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await BuildupSelectionVM.HandleBuilupSelectionChangedAsync();
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            if (expander != null)
            {
                this.Width += 670; // border width + margin
                this.Left = originalLeft; // Keep the left position fixed
            }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            if (expander != null)
            {
                this.Width -= 670;
                this.Left = originalLeft; // Keep the left position fixed
            }
        }


    }
}
