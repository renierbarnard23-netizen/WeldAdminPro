using System.Windows;

namespace WeldAdminPro.UI.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

			MessageBox.Show("Login Window Loaded"); // 🔥 TEST
		}

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text;
            var password = PasswordBox.Password;

			if (username == "admin" && password == "admin")
			{
				var mainWindow = new MainWindow();

				Application.Current.MainWindow = mainWindow;

				mainWindow.Show();

				this.Close(); // OK now
			}
			else
            {
                ErrorText.Text = "Invalid username or password";
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
