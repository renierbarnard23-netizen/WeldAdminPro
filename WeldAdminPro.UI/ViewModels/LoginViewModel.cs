using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Core.Security;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUserRepository _userRepository;

        public event Action? LoginSucceeded;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public LoginViewModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            var user = await _userRepository.GetByUsernameAsync(Username);

            if (user == null ||
                !AuthService.VerifyPassword(user.PasswordHash ?? string.Empty, Password))
            {
                ErrorMessage = "Invalid username or password";
                return;
            }

            LoginSucceeded?.Invoke();
        }
    }
}
