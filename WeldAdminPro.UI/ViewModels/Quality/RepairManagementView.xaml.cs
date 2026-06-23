using System.Windows;
using System.Windows.Controls;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Services;
using WeldAdminPro.UI.ViewModels.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class RepairManagementView
        : UserControl
    {
        public RepairManagementView()
        {
            InitializeComponent();

            DataContext =
                new RepairManagementViewModel();
        }

        // =====================================
        // APPROVE REPAIR
        // =====================================

        private void ApproveRepair_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!PermissionService.HasPermission(
                SystemPermission.ApproveWeldRepairs))
            {
                MessageBox.Show(
                    "You do not have permission to approve repairs.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // TODO:
            // Add your actual repair approval logic here

            AuditService.Log(
                "APPROVE REPAIR",
                "Quality",
                "Repair approved");
        }

        // =====================================
        // VERIFY REPAIR
        // =====================================

        private void VerifyRepair_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!PermissionService.HasPermission(
                SystemPermission.VerifyRepairs))
            {
                MessageBox.Show(
                    "You do not have permission to verify repairs.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // TODO:
            // Add your actual repair verification logic here

            AuditService.Log(
                "VERIFY REPAIR",
                "Quality",
                "Repair verified");
        }
    }
}