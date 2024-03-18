﻿using Calc.Core.Objects;
using Calc.MVVM.Helpers;
using Calc.MVVM.Helpers.Mediators;
using Calc.MVVM.Models;
using Calc.MVVM.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calc.MVVM.Views
{
    public partial class BuilderView : Window
    {
        private readonly BuilderViewModel BuilderVM;

        public BuilderView(BuilderViewModel bvm)
        {
            BuilderVM = bvm;
            this.DataContext = BuilderVM;
            InitializeComponent();
            MediatorToView.Register("ViewDeselectTreeView", _=>DeselectTreeView());
        }

        private void SelectClicked(object sender, RoutedEventArgs e)
        {
            // hide the window when selecting elelemts
            this.Hide();
            BuilderVM.HandleSelect();
            // show the window again
            this.Show();
        }
        private void TreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            if (TreeView.SelectedItem is ICalcComponent selectedCompo)
            {
                BuilderVM.HandleComponentSelectionChanged(selectedCompo);
                TreeView.Tag = e.OriginalSource; // save selected treeviewitem for deselecting
                e.Handled = true;
            }
        }

        private void DeselectTreeView()
        {
            if (TreeView.SelectedItem != null)
            {
                if (TreeView.Tag is TreeViewItem selectedTreeViewItem)
                {
                    selectedTreeViewItem.IsSelected = false;
                }
            }
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            await BuilderVM.HandleWindowLoadedAsync();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BuilderVM.HandleWindowClosing();
        }


        
        private void SideClickDown(object sender, MouseButtonEventArgs e)
        {
            BuilderVM.HandleSideClicked();
        }


        private void RemoveClicked(object sender, RoutedEventArgs e)
        {
            BuilderVM.HandleSelect();
        }


        private void MessageOKClicked(object sender, RoutedEventArgs e)
        {
            BuilderVM.HandleMessageClose();
        }


        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BuildupNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
