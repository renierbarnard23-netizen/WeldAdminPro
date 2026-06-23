using System.Windows.Controls;
using WeldAdminPro.UI.ViewModels;

namespace WeldAdminPro.UI.Views
{
    public partial class AuditLogView : UserControl
    {
        public AuditLogView()
        {
            InitializeComponent();

            DataContext =
                new AuditLogViewModel();
        }
    }
}