using System.Windows;
using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels.Quality; // ✅ THIS LINE FIXES IT
using WeldAdminPro.Core.Services;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class WpsView : UserControl
    {
        public WpsView()
        {
            InitializeComponent();
            DataContext = new WpsViewModel(); // ✅ now it resolves
        }
    }
}