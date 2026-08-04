using WeldAdminPro.Core.Security;

namespace WeldAdminPro.Core.Security.Catalog;

public static class RolePermissionCatalog
{
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> All =>
        new Dictionary<string, IReadOnlyList<string>>
        {
            // ======================================================
            // ADMINISTRATOR
            // ======================================================

            ["Administrator"] =
                PermissionCatalog.All
                    .Select(x => x.Key)
                    .ToList(),

            // ======================================================
            // OPERATIONS MANAGER
            // ======================================================

            ["Operations Manager"] =
            [
                PermissionKeys.Inventory.View,
                PermissionKeys.Inventory.Forecast,
                PermissionKeys.Inventory.Export,

                PermissionKeys.Projects.View,
                PermissionKeys.Projects.Create,
                PermissionKeys.Projects.Edit,
                PermissionKeys.Projects.Costs,
                PermissionKeys.Projects.Compliance,
                PermissionKeys.Projects.Profitability,
                PermissionKeys.Projects.Risk,

                PermissionKeys.Production.View,
                PermissionKeys.Production.WorkOrders,
                PermissionKeys.Production.CreateWorkOrder,
                PermissionKeys.Production.EditWorkOrder,
                PermissionKeys.Production.IssueMaterial,
                PermissionKeys.Production.Start,
                PermissionKeys.Production.Complete,
                PermissionKeys.Production.Schedule,

                PermissionKeys.Procurement.View,
                PermissionKeys.Procurement.CreatePO,
                PermissionKeys.Procurement.EditPO,
                PermissionKeys.Procurement.ApprovePO,
                PermissionKeys.Procurement.ReceiveGoods,
                PermissionKeys.Procurement.Export,

                PermissionKeys.Quality.View,

                PermissionKeys.Reports.View,
                PermissionKeys.Reports.Export
            ],

            // ======================================================
            // QUALITY MANAGER
            // ======================================================

            ["Quality Manager"] =
            [
                PermissionKeys.Projects.View,
                PermissionKeys.Projects.Compliance,
                PermissionKeys.Projects.Risk,

                PermissionKeys.Production.View,
                PermissionKeys.Production.WorkOrders,

                PermissionKeys.Quality.View,
                PermissionKeys.Quality.WPS,
                PermissionKeys.Quality.PQR,
                PermissionKeys.Quality.WeldRegister,
                PermissionKeys.Quality.Repairs,
                PermissionKeys.Quality.NCR,
                PermissionKeys.Quality.NcrDisposition,
                PermissionKeys.Quality.NcrVerify,
                PermissionKeys.Quality.NcrClose,
                PermissionKeys.Quality.NDT,
                PermissionKeys.Quality.Export,

                PermissionKeys.Reports.View,
                PermissionKeys.Reports.Export,
                PermissionKeys.Quality.HoldPointApproval
            ],

            // ======================================================
            // WELDING COORDINATOR
            // ======================================================

            ["Welding Coordinator"] =
            [
                PermissionKeys.Projects.View,
                PermissionKeys.Projects.Compliance,

                PermissionKeys.Production.View,
                PermissionKeys.Production.WorkOrders,

                PermissionKeys.Quality.View,
                PermissionKeys.Quality.WPS,
                PermissionKeys.Quality.PQR,
                PermissionKeys.Quality.WeldRegister,
                PermissionKeys.Quality.Repairs,
                PermissionKeys.Quality.NCR,
                PermissionKeys.Quality.NcrDisposition,
                PermissionKeys.Quality.NcrVerify,
                PermissionKeys.Quality.NcrClose,
                PermissionKeys.Quality.NDT,
                PermissionKeys.Quality.Export,

                PermissionKeys.Reports.View,
                PermissionKeys.Quality.HoldPointApproval
            ],

            // ======================================================
            // PRODUCTION SUPERVISOR
            // ======================================================

            ["Production Supervisor"] =
            [
                PermissionKeys.Inventory.View,
                PermissionKeys.Inventory.StockOut,

                PermissionKeys.Projects.View,

                PermissionKeys.Production.View,
                PermissionKeys.Production.WorkOrders,
                PermissionKeys.Production.EditWorkOrder,
                PermissionKeys.Production.IssueMaterial,
                PermissionKeys.Production.Start,
                PermissionKeys.Production.Complete,

                PermissionKeys.Quality.View,
                PermissionKeys.Quality.WeldRegister,

                PermissionKeys.Reports.View
            ],

            // ======================================================
            // QA INSPECTOR
            // ======================================================

            ["QA Inspector"] =
            [
                PermissionKeys.Projects.View,
                PermissionKeys.Projects.Compliance,

                PermissionKeys.Production.View,
                PermissionKeys.Production.WorkOrders,

                PermissionKeys.Quality.View,
                PermissionKeys.Quality.WPS,
                PermissionKeys.Quality.PQR,
                PermissionKeys.Quality.WeldRegister,
                PermissionKeys.Quality.Repairs,
                PermissionKeys.Quality.NCR,
                PermissionKeys.Quality.NcrDisposition,
                PermissionKeys.Quality.NcrVerify,
                PermissionKeys.Quality.NcrClose,
                PermissionKeys.Quality.NDT,
                PermissionKeys.Quality.Export,

                PermissionKeys.Reports.View,
                PermissionKeys.Quality.HoldPointApproval
            ],

            // ======================================================
            // QC INSPECTOR
            // ======================================================

            ["QC Inspector"] =
            [
                PermissionKeys.Projects.View,

                PermissionKeys.Production.View,
                PermissionKeys.Production.WorkOrders,

                PermissionKeys.Quality.View,
                PermissionKeys.Quality.WPS,
                PermissionKeys.Quality.WeldRegister,
                PermissionKeys.Quality.Repairs,
                PermissionKeys.Quality.NCR,
                PermissionKeys.Quality.NcrVerify,
                PermissionKeys.Quality.NDT,

                PermissionKeys.Reports.View,
                PermissionKeys.Quality.HoldPointApproval
            ],

            // ======================================================
            // STORE CONTROLLER
            // ======================================================

            ["Store Controller"] =
            [
                PermissionKeys.Inventory.View,
                PermissionKeys.Inventory.Create,
                PermissionKeys.Inventory.Edit,
                PermissionKeys.Inventory.StockIn,
                PermissionKeys.Inventory.StockOut,
                PermissionKeys.Inventory.Forecast,
                PermissionKeys.Inventory.Export,

                PermissionKeys.Projects.View,

                PermissionKeys.Production.View,
                PermissionKeys.Production.WorkOrders,
                PermissionKeys.Production.IssueMaterial,

                PermissionKeys.Procurement.View,
                PermissionKeys.Procurement.CreatePO,
                PermissionKeys.Procurement.EditPO,
                PermissionKeys.Procurement.ReceiveGoods,
                PermissionKeys.Procurement.Export,

                PermissionKeys.Reports.View
            ],

            // ======================================================
            // WELDER
            // ======================================================

            ["Welder"] =
            [
                PermissionKeys.Projects.View,

                PermissionKeys.Production.View,
                PermissionKeys.Production.WorkOrders,
                PermissionKeys.Production.Start,
                PermissionKeys.Production.Complete,

                PermissionKeys.Quality.View,
                PermissionKeys.Quality.WPS,
                PermissionKeys.Quality.WeldRegister
            ],

            // ======================================================
            // VIEWER
            // ======================================================

            ["Viewer"] =
            [
                PermissionKeys.Inventory.View,
                PermissionKeys.Projects.View,
                PermissionKeys.Production.View,
                PermissionKeys.Production.WorkOrders,
                PermissionKeys.Procurement.View,
                PermissionKeys.Quality.View,
                PermissionKeys.Reports.View
            ]
        };
}