using System;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services;

namespace WeldAdminPro.UI.Views
{
    public partial class EditUserWindow : Window
    {
        private readonly PasswordHashService
            _hashService = new();

        public SystemUser User { get; }

        public Array Roles =>
            Enum.GetValues(typeof(SystemRole));

        public EditUserWindow(SystemUser user)
        {
            InitializeComponent();

            User = user;

            DataContext = this;
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

        public SystemRole Role
        {
            get => User.Role;
            set => User.Role = value;
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