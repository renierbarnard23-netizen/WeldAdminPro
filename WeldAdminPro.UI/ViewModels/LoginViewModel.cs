// File: WeldAdminPro.UI\ViewModels/LoginViewModel.cs
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Data.Repositories;
// If your AuthService lives in a different namespace, change this using accordingly:
using WeldAdminPro.Core.Security;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUserRepository _userRepo;

        public LoginViewModel(IUserRepository userRepo)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync);
        }

        // Bindable properties
        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private bool rememberMe;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        // Not exposed to UI as a bindable: password is forwarded from code-behind
        private string _password = string.Empty;

        // Public command bound by XAML
        public IAsyncRelayCommand LoginCommand { get; }

        // Called by the view's code-behind (PasswordBox.PasswordChanged)
        public void SetPassword(string plainPassword)
        {
            _password = plainPassword ?? string.Empty;
        }

        // Simple success flag you can bind to or react to
        [ObservableProperty]
        private bool isAuthenticated;

        // Optional event to let the view know login succeeded (you can also do navigation via DI)
        public event Action? LoginSucceeded;

        private async Task ExecuteLoginAsync()
        {
            StatusMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username))
            {
                StatusMessage = "Please enter username.";
                return;
            }

            try
            {
                var user = await _userRepo.GetByUsernameAsync(Username);
                if (user == null)
                {
                    StatusMessage = "Invalid username or password.";
                    return;
                }

                // Verify password using your AuthService (adjust namespace if necessary)
                var ok = AuthService.VerifyPassword(user.PasswordHash ?? string.Empty, _password);
                if (!ok)
                {
                    StatusMessage = "Invalid username or password.";
                    return;
                }

                // Successful login
                IsAuthenticated = true;
                StatusMessage = "Login successful.";
                LoginSucceeded?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = "Login error: " + ex.Message;
            }
        }
    }
}
