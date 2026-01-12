using System.Windows;

namespace WeldAdminPro.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenProjects_Click(object sender, RoutedEventArgs e)
        {
            var projectsView = new ProjectsView
            {
                Owner = this
            };

            projectsView.Show();
        }
    }
}
