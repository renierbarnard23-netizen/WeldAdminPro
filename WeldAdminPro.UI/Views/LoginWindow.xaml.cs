using System.Windows;

namespace WeldAdminPro.UI.Views
{
	public partial class LoginWindow : Window
	{
		public LoginWindow()
		{
			InitializeComponent();
		}

		private void Login_Click(object sender, RoutedEventArgs e)
		{
			var username = UsernameBox.Text;
			var password = PasswordBox.Password;

			if (username == "admin" && password == "admin")
			{
				DialogResult = true;
				Close(); // 🔥 IMPORTANT
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