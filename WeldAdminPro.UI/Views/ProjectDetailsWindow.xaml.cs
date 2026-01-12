using System.Windows;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.UI.Views
{
    public partial class ProjectDetailsWindow : Window
    {
        public ProjectDetailsWindow(Project project)
        {
            InitializeComponent();

            TitleText.Text = project.ProjectName;
            DetailsGrid.ItemsSource = new[] { project };
        }
    }
}
