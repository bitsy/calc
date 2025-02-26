﻿using Calc.MVVM.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Calc.MVVM.Views
{
    public partial class LoginView : Window
    {
        private readonly LoginViewModel LoginVM;

        public LoginView(LoginViewModel viewModel)
        {
            this.LoginVM = viewModel;
            this.DataContext = LoginVM;
            InitializeComponent();
            this.PasswordBox.Password = LoginVM.Password;            
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            bool c = await LoginVM.HandleAutoLogin();
            if (c) this.Close();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LoginVM.CancelLoad();        
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            LoginVM.Password = PasswordBox.Password;
        }

        private async void OkClicked(object sender, RoutedEventArgs e)
        {
            bool c = await LoginVM.HandleOK();
            if (c) this.Close();
        }
    }
}
