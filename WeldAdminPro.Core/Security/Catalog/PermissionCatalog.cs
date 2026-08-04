using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Definitions;


namespace WeldAdminPro.Core.Security.Catalog;

public static class PermissionCatalog
{
    public static IReadOnlyList<PermissionDefinition> All =>
    [
        // ==========================================================
        // INVENTORY
        // ==========================================================

        new(
            PermissionKeys.Inventory.View,
            PermissionGroups.Inventory,
            "View",
            "View inventory"),

        new(
            PermissionKeys.Inventory.Create,
            PermissionGroups.Inventory,
            "Create",
            "Create inventory items"),
        new(
            PermissionKeys.Inventory.Edit,
            PermissionGroups.Inventory,
            "Edit",
            "Edit inventory items"),
        new(
            PermissionKeys.Inventory.Delete,
            PermissionGroups.Inventory,
            "Delete",
            "Delete inventory items"),
        new(
            PermissionKeys.Inventory.StockIn,
            PermissionGroups.Inventory,
            "Stock In",
            "Receive stock"),
        new(
            PermissionKeys.Inventory.StockOut,
            PermissionGroups.Inventory,
            "Stock Out",
            "Issue stock"),
        new(
            PermissionKeys.Inventory.Forecast,
            PermissionGroups.Inventory,
            "Forecast",
            "Inventory forecasting"),
        new(
            PermissionKeys.Inventory.Export,
            PermissionGroups.Inventory,
            "Export",
            "Export inventory"),

        // ==========================================================
        // PROJECTS
        // ==========================================================

        new(
            PermissionKeys.Projects.View,
            PermissionGroups.Projects,
            "View",
            "View projects"),

        new(
            PermissionKeys.Projects.Create,
            PermissionGroups.Projects,
            "Create",
            "Create projects"),
        new(
            PermissionKeys.Projects.Edit,
            PermissionGroups.Projects,
            "Edit",
            "Edit projects"),
        new(
            PermissionKeys.Projects.Delete,
            PermissionGroups.Projects,
            "Delete",
            "Delete projects"),
        new(
            PermissionKeys.Projects.Costs,
            PermissionGroups.Projects,
            "Costs",
            "View project costs"),
        new(
            PermissionKeys.Projects.Compliance,
            PermissionGroups.Projects,
            "Compliance",
            "Project compliance"),
        new(
            PermissionKeys.Projects.Profitability,
            PermissionGroups.Projects,
            "Profitability",
            "View profitability"),
        new(
            PermissionKeys.Projects.Risk,
            PermissionGroups.Projects,
            "Risk",
            "Project risk dashboard"),

        // ==========================================================
        // PRODUCTION
        // ==========================================================

        new(
            PermissionKeys.Production.View,
            PermissionGroups.Production,
            "View",
            "View production"),
        new(
            PermissionKeys.Production.WorkOrders,
            PermissionGroups.Production,
            "Work Orders",
            "View work orders"),
        new(
            PermissionKeys.Production.CreateWorkOrder,
            PermissionGroups.Production,
            "Create",
            "Create work orders"),
        new(
            PermissionKeys.Production.EditWorkOrder,
            PermissionGroups.Production,
            "Edit",
            "Edit work orders"),
        new(
            PermissionKeys.Production.IssueMaterial,
            PermissionGroups.Production,
            "Issue Material",
            "Issue material"),
        new(
            PermissionKeys.Production.Start,
            PermissionGroups.Production,
            "Start",
            "Start work order"),
        new(
            PermissionKeys.Production.Complete,
            PermissionGroups.Production,
            "Complete",
            "Complete work order"),
        new(
            PermissionKeys.Production.Schedule,
            PermissionGroups.Production,
            "Schedule",
            "Production scheduling"),

        // ==========================================================
        // PROCUREMENT
        // ==========================================================

        new(
            PermissionKeys.Procurement.View,
            PermissionGroups.Procurement,
            "View",
            "View procurement"),
        new(
            PermissionKeys.Procurement.CreatePO,
            PermissionGroups.Procurement,
            "Create PO",
            "Create purchase orders"),
        new(
            PermissionKeys.Procurement.EditPO,
            PermissionGroups.Procurement,
            "Edit PO",
            "Edit purchase orders"),
        new(
            PermissionKeys.Procurement.ApprovePO,
            PermissionGroups.Procurement,
            "Approve PO",
            "Approve purchase orders"),
        new(
            PermissionKeys.Procurement.ReceiveGoods,
            PermissionGroups.Procurement,
            "Receive",
            "Receive goods"),
        new(
            PermissionKeys.Procurement.Export,
            PermissionGroups.Procurement,
            "Export",
            "Export procurement"),

        // ==========================================================
        // QUALITY
        // ==========================================================

        new(
            PermissionKeys.Quality.View,
            PermissionGroups.Quality,
            "View",
            "View quality"),
        new(
            PermissionKeys.Quality.WPS,
            PermissionGroups.Quality,
            "WPS",
            "Manage WPS"),
        new(
            PermissionKeys.Quality.PQR,
            PermissionGroups.Quality,
            "PQR",
            "Manage PQR"),
        new(
            PermissionKeys.Quality.WeldRegister,
            PermissionGroups.Quality,
            "Weld Register",
            "Manage weld register"),
        new(
            PermissionKeys.Quality.Repairs,
            PermissionGroups.Quality,
            "Repairs",
            "Manage repairs"),
        new(
            PermissionKeys.Quality.NCR,
            PermissionGroups.Quality,
            "NCR",
            "Manage non-conformance reports"),
        new(
            PermissionKeys.Quality.NcrDisposition,
            PermissionGroups.Quality,
            "NCR Disposition",
            "Approve NCR dispositions"),
        new(
            PermissionKeys.Quality.NcrVerify,
            PermissionGroups.Quality,
            "NCR Verification",
            "Verify NCR corrective actions"),
        new(
            PermissionKeys.Quality.NcrClose,
            PermissionGroups.Quality,
            "NCR Close",
            "Close verified NCRs"),
        new(
            PermissionKeys.Quality.NDT,
            PermissionGroups.Quality,
            "NDT",
            "Manage NDT"),
        new(
            PermissionKeys.Quality.HoldPointApproval,
            PermissionGroups.Quality,
            "Hold Point Approval",
            "Approve and reject quality hold points"),
        new(
            PermissionKeys.Quality.Export,
            PermissionGroups.Quality,
            "Export",
            "Export quality"),

        // ==========================================================
        // ADMINISTRATION
        // ==========================================================

        new(
            PermissionKeys.Administration.Users,
            PermissionGroups.Administration,
            "Users",
            "Manage users"),
        new(
            PermissionKeys.Administration.AuditLog,
            PermissionGroups.Administration,
            "Audit Log",
            "View audit log"),
        new(
            PermissionKeys.Administration.Security,
            PermissionGroups.Administration,
            "Security",
            "Manage permissions"),
        new(
            PermissionKeys.Administration.Settings,
            PermissionGroups.Administration,
            "Settings",
            "System settings"),

        // ==========================================================
        // REPORTS
        // ==========================================================

        new(
            PermissionKeys.Reports.View,
            PermissionGroups.Reports,
            "View",
            "View reports"),
        new(
            PermissionKeys.Reports.Export,
            PermissionGroups.Reports,
            "Export",
            "Export reports")
    ];    
}