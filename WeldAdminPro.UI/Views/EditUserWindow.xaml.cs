using System;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Security.Models;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories.Security;

namespace WeldAdminPro.UI.Views
{
    public partial class EditUserWindow : Window
    {
        private readonly PasswordHashService
            _hashService = new();

        private readonly RoleRepository _roleRepository;

        public SystemUser User { get; }

        public System.Collections.Generic.IReadOnlyList<Role> Roles { get; private set; } =
            Array.Empty<Role>();

        public Role? SelectedRole { get; set; }

        public EditUserWindow(SystemUser user)
        {
            InitializeComponent();

            User = user;

            _roleRepository =
                new RoleRepository(
                    DatabasePath.GetConnectionString());

            LoadRoles();

            DataContext = this;
        }

        private void LoadRoles()
        {
            Roles =
                _roleRepository
                    .GetAllAsync()
                    .GetAwaiter()
                    .GetResult()
                    .ToList();

            SelectedRole =
                Roles.FirstOrDefault(
                    r => r.Id == User.RoleId);
        }

        public string Username
        {
            get => User.Username;
            set => User.Username = value;
        }

        public string FullName
        {
            get => User.FullName;
            set => User.FullName = value;
        }

        public string Email
        {
            get => User.Email;
            set => User.Email = value;
        }

        public int RoleId
        {
            get => User.RoleId;
            set => User.RoleId = value;
        }

        public bool UserIsActive
        {
            get => User.IsActive;
            set => User.IsActive = value;
        }

        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(
                PasswordBox.Password))
            {
                User.PasswordHash =
                    _hashService.Hash(
                        PasswordBox.Password);
            }

            if (SelectedRole == null)
            {
                MessageBox.Show(
                    "Please select a role.",
                    "Role Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            User.RoleId = SelectedRole.Id;
            User.RoleName = SelectedRole.Name;

            DialogResult = true;
        }

        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
