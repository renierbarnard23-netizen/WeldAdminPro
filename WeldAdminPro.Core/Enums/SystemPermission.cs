namespace WeldAdminPro.Core.Enums
{
    public enum SystemPermission
    {
        // =====================================
        // GENERAL ACCESS
        // =====================================

        AccessQuality,
        AccessProduction,
        AccessReports,
        AccessAuditLogs,

        // =====================================
        // USER & SECURITY
        // =====================================

        ManageUsers,
        ManagePermissions,

        // =====================================
        // WELDING CONTROL
        // =====================================

        ManageWps,
        ManagePqr,
        ReleaseWeld,

        ApproveRepairs,
        CloseNcr,

        ApproveNcrDisposition,
        VerifyNcr,
        CloseNcrs,

        ApproveWeldRepairs,
        VerifyRepairs,     

        ApproveNcr,
        ApproveCapa,

        // =====================================
        // DOCUMENT CONTROL
        // =====================================

        AccessDocumentVault,
        UploadDocuments,
        DeleteDocuments,

        // =====================================
        // PRODUCTION
        // =====================================

        ViewWorkOrders,
        CreateWorkOrders,
        EditWorkOrders,
        CloseWorkOrders,

        // =====================================
        // STOCK
        // =====================================

        ManageStock,
        ApproveStockIssues,

        // =====================================
        // EXECUTIVE
        // =====================================

        ViewExecutiveDashboards,
        ViewFinancials,

        // =====================================
        // WPS
        // =====================================

        ViewWps,
        CreateWps,
        EditWps,
        DeleteWps,
        ApproveWps,
    }
}