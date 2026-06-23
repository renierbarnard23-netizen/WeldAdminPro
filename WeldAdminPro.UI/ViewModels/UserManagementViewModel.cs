using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.UI.Views;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class UserManagementViewModel
        : ObservableObject
    {
        private readonly SystemUserRepository
            _repository;

        private readonly PasswordHashService
            _hashService = new();

        [ObservableProperty]
        private ObservableCollection<SystemUser>
            users = new();

        [ObservableProperty]
        private SystemUser? selectedUser;

        public UserManagementViewModel()
        {
            _repository =
                new SystemUserRepository(
                    DatabasePath.GetConnectionString());

            EditUserCommand =
                new RelayCommand(EditUser);

            EnableUserCommand =
                new RelayCommand(EnableUser);

            Load();
        }
        public ICommand EditUserCommand { get; }
        public ICommand EnableUserCommand { get; }

        public void Load()
        {
            var users =
                _repository.GetAll();

            Users =
                new ObservableCollection<SystemUser>(
                    users);
        }

        private void EditUser()
        {
            if (SelectedUser == null)
                return;

            var window =
                new EditUserWindow(
                    SelectedUser);

            var result =
                window.ShowDialog();

            if (result == true)
            {
                _repository.Update(
                    SelectedUser);

                AuditService.Log(
"EDIT USER",
"User Management",
$"Edited user: {SelectedUser.Username}");

                Load();

                MessageBox.Show(
                    "User updated.");
            }
        }

        private void EnableUser()
        {
            if (SelectedUser == null)
                return;

            SelectedUser.IsActive = true;

            _repository.Update(
                SelectedUser);

            AuditService.Log(
                "ENABLE USER",
                "User Management",
                $"Enabled user: {SelectedUser.Username}");

            Load();

            MessageBox.Show(
                "User enabled.");
        }

        [RelayCommand]
        private void AddUser()
        {
            var user =
    new SystemUser
    {
        Id = Guid.NewGuid(),

        Username = "newuser",

        FullName = "New User",

        Email = "",

        PasswordHash =
            _hashService.Hash("password123"),

        Role = SystemRole.Viewer,

        IsActive = true,

        CreatedDate = DateTime.UtcNow
    };

            _repository.Add(user);

            AuditService.Log(
    "CREATE USER",
    "User Management",
    $"Created user: {user.Username}");

            Load();

            MessageBox.Show(
                "User created.");
        }

        [RelayCommand]
        private void DisableUser()
        {
            if (SelectedUser == null)
                return;

            SelectedUser.IsActive = false;

            _repository.Update(
                SelectedUser);

            AuditService.Log(
    "DISABLE USER",
    "User Management",
    $"Disabled user: {SelectedUser.Username}");

            Load();

            MessageBox.Show(
                "User disabled.");
        }

        [RelayCommand]
        private void ResetPassword()
        {
            if (SelectedUser == null)
                return;

            SelectedUser.PasswordHash =
                _hashService.Hash(
                    "password123");

            _repository.Update(
                SelectedUser);

            AuditService.Log(
    "RESET PASSWORD",
    "User Management",
    $"Password reset for: {SelectedUser.Username}");

            MessageBox.Show(
                "Password reset to password123");
        }
    }
}
