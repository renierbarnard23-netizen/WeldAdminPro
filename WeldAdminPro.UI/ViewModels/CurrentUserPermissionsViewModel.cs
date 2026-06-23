using System.Windows;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Services;

namespace WeldAdminPro.UI.ViewModels
{
    public class CurrentUserPermissionsViewModel
    {
        // =====================================
        // QUALITY
        // =====================================

        public Visibility QualityVisibility =>
            PermissionService.HasPermission(
                SystemPermission.AccessQuality)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

        // =====================================
        // PRODUCTION
        // =====================================

        public Visibility ProductionVisibility =>
            PermissionService.HasPermission(
                SystemPermission.ViewWorkOrders)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

        // =====================================
        // REPORTS
        // =====================================

        public Visibility ReportsVisibility =>
            PermissionService.HasPermission(
                SystemPermission.AccessReports)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

        // =====================================
        // USER MANAGEMENT
        // =====================================

        public Visibility AdminVisibility =>
            PermissionService.HasPermission(
                SystemPermission.ManageUsers)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

        // =====================================
        // AUDIT LOGS
        // =====================================

        public Visibility AuditVisibility =>
            PermissionService.HasPermission(
                SystemPermission.AccessAuditLogs)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

        // =====================================
        // DOCUMENT VAULT
        // =====================================

        public Visibility DocumentVaultVisibility =>
            PermissionService.HasPermission(
                SystemPermission.AccessDocumentVault)
                    ? Visibility.Visible
                    : Visibility.Collapsed;

        // =====================================
        // EXECUTIVE
        // =====================================

        public Visibility ExecutiveVisibility =>
            PermissionService.HasPermission(
                SystemPermission.ViewExecutiveDashboards)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
    }
}