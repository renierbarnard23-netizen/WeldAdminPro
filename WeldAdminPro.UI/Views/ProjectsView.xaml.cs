using System.Windows;

namespace WeldAdminPro.UI.Views
{
    public partial class ProjectsView : Window
    {
        public ProjectsView()
        {
            InitializeComponent();
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var win = new NewProjectWindow();
            win.ShowDialog();
        }

        private void EditProject_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void ProjectsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO
        }

        public void Refresh()
        {
            // TODO
        }
    }
}
