using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            try
            {
                InitializeComponent();

                // Optional: attach Loaded here instead of XAML if you prefer
                // this.Loaded += LoginWindow_Loaded;
            }
            catch (Exception ex)
            {
                try
                {
                    File.WriteAllText(
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LoginWindow_init_error.txt"),
                        ex.ToString()
                    );
                }
                catch { /* swallow file write errors */ }

                throw;
            }
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // DataContext should have been set by App via DI:
            if (DataContext is LoginViewModel vm)
            {
                // make sure we don't double-subscribe (if using DI + Loaded firing multiple times)
                vm.LoginSucceeded -= OnLoginSucceeded;
                vm.LoginSucceeded += OnLoginSucceeded;
            }
        }

        private void OnLoginSucceeded()
        {
            // Close on UI thread
            Dispatcher.Invoke(() => this.Close());
        }

        // Called when user types into password field
        private void PwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
            {
                vm.SetPassword(pb.Password);
            }
        }

        // Cancel button
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
