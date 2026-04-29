using System.Windows;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class PqrEditorView : Window
    {
        public PqrEditorView()
        {
            InitializeComponent();
            DataContext = new PqrEditorViewModel();
        }
    }
}