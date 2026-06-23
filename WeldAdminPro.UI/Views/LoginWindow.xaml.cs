using System.Windows;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;


namespace WeldAdminPro.UI.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(
    object sender,
    RoutedEventArgs e)
        {
            var repository =
                new SystemUserRepository(
                    DatabasePath.GetConnectionString());

            var authService =
                new AuthenticationService(
                    repository);

            var user =
                authService.Authenticate(
                    UsernameTextBox.Text,
                    PasswordBox.Password);

            if (user == null)
            {
                MessageBox.Show(
                    "Invalid username or password.");

                return;
            }

            /// SESSION

            CurrentUserService.CurrentUser =
                user;

            CurrentUserContext.Username =
                user.Username;

            CurrentUserContext.FullName =
                user.FullName;

            if (Enum.TryParse(
                user.Role.ToString(),
                true,
                out WeldAdminPro.Core.Enums.SystemRole parsedRole))
            {
                CurrentUserContext.Role =
                    parsedRole;
            }
            else
            {
                CurrentUserContext.Role =
                    WeldAdminPro.Core.Enums.SystemRole.Viewer;
            }

            CurrentUserContext.IsAuthenticated =
                true;

            try
            {
                AuditService.Log(
                    "LOGIN",
                    "Authentication",
                    $"User logged in: {user.Username}");
            }
            catch
            {
                // prevent login crash
            }

            DialogResult = true;

            Close();
        }

        private void Exit_Click(
                object sender,
                RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

    }
}
