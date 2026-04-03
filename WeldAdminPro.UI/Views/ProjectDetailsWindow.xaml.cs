using System.Windows;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
	public partial class ProjectDetailsWindow : Window
	{
		public ProjectDetailsWindow(ProjectDetailsViewModel viewModel)
		{
			InitializeComponent();

			DataContext = viewModel;

			viewModel.RequestClose += () =>
			{
				DialogResult = true;
				Close();
			};
		}

		private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{

		}
	}
}