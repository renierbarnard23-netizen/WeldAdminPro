using System.Collections.Generic;
using WeldAdminPro.Core.Enums;

namespace WeldAdminPro.Core.Services
{
    public static class RolePermissionMatrix
    {
        public static readonly Dictionary<SystemRole,
            List<SystemPermission>> Permissions =
                new()
                {
                    // =====================================
                    // ADMIN
                    // =====================================

                    {
                        SystemRole.Admin,
                        new List<SystemPermission>
                        {
                            SystemPermission.AccessQuality,
                            SystemPermission.AccessProduction,
                            SystemPermission.AccessReports,
                            SystemPermission.AccessAuditLogs,

                            SystemPermission.ManageUsers,
                            SystemPermission.ManagePermissions,

                            SystemPermission.ManageWps,
                            SystemPermission.ManagePqr,
                            SystemPermission.ReleaseWeld,
                            SystemPermission.ApproveRepairs,
                            SystemPermission.CloseNcr,

                            SystemPermission.AccessDocumentVault,
                            SystemPermission.UploadDocuments,
                            SystemPermission.DeleteDocuments,

                            SystemPermission.CreateWorkOrders,
                            SystemPermission.EditWorkOrders,
                            SystemPermission.CloseWorkOrders,

                            SystemPermission.ManageStock,
                            SystemPermission.ApproveStockIssues,

                            SystemPermission.ViewExecutiveDashboards,
                            SystemPermission.ViewFinancials,

                            SystemPermission.ViewWps,
                            SystemPermission.CreateWps,
                            SystemPermission.EditWps,
                            SystemPermission.DeleteWps,
                            SystemPermission.ApproveWps,
                            SystemPermission.ViewWorkOrders,

                            SystemPermission.ApproveWeldRepairs,
                            SystemPermission.VerifyRepairs,
                            SystemPermission.ApproveNcr,
                            SystemPermission.CloseNcr,
                            SystemPermission.ApproveCapa,

                            SystemPermission.ApproveNcrDisposition,
                            SystemPermission.VerifyNcr,
                            SystemPermission.CloseNcrs,

                        }
                    },

                    // =====================================
                    // QA
                    // =====================================

                    {
                        SystemRole.QA,
                        new List<SystemPermission>
                        {
                            SystemPermission.AccessQuality,
                            SystemPermission.AccessReports,

                            SystemPermission.ManageWps,
                            SystemPermission.ManagePqr,
                            SystemPermission.ReleaseWeld,
                            SystemPermission.ApproveRepairs,
                            SystemPermission.CloseNcr,

                            SystemPermission.AccessDocumentVault,
                            SystemPermission.UploadDocuments,

                            SystemPermission.ViewWps,
                            SystemPermission.CreateWps,
                            SystemPermission.EditWps,
                            SystemPermission.ApproveWps,
                            SystemPermission.ViewWorkOrders,

                            SystemPermission.ApproveWeldRepairs,
                            SystemPermission.VerifyRepairs,
                            SystemPermission.ApproveNcr,
                            SystemPermission.CloseNcr,
                            SystemPermission.ApproveCapa,

                            SystemPermission.ApproveNcrDisposition,
                            SystemPermission.VerifyNcr,
                            SystemPermission.CloseNcrs,

                        }
                    },

                    // =====================================
                    // QC
                    // =====================================

                    {
                        SystemRole.QC,
                        new List<SystemPermission>
                        {
                            SystemPermission.AccessQuality,

                            SystemPermission.ReleaseWeld,

                            SystemPermission.AccessDocumentVault,

                            SystemPermission.ViewWps,

                            SystemPermission.VerifyRepairs,
                        }
                    },

                    // =====================================
                    // STORE CONTROLLER
                    // =====================================

                    {
                        SystemRole.StoreController,
                        new List<SystemPermission>
                        {
                            SystemPermission.AccessProduction,

                            SystemPermission.ManageStock,
                            SystemPermission.ApproveStockIssues,

                            SystemPermission.CreateWorkOrders,

                            SystemPermission.ViewWorkOrders,
                            SystemPermission.CreateWorkOrders,
                        }
                    },

                    // =====================================
                    // VIEWER
                    // =====================================

                    {
                        SystemRole.Viewer,
                        new List<SystemPermission>()
                    }
                };
    }
}