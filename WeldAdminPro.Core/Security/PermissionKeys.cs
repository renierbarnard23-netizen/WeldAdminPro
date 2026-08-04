namespace WeldAdminPro.Core.Security;

public static class PermissionKeys
{
    public static class Inventory
    {
        public const string View = "Inventory.View";
        public const string Create = "Inventory.Create";
        public const string Edit = "Inventory.Edit";
        public const string Delete = "Inventory.Delete";

        public const string StockIn = "Inventory.StockIn";
        public const string StockOut = "Inventory.StockOut";

        public const string Forecast = "Inventory.Forecast";
        public const string Export = "Inventory.Export";
    }

    public static class Projects
    {
        public const string View = "Projects.View";
        public const string Create = "Projects.Create";
        public const string Edit = "Projects.Edit";
        public const string Delete = "Projects.Delete";

        public const string Costs = "Projects.Costs";
        public const string Compliance = "Projects.Compliance";
        public const string Profitability = "Projects.Profitability";
        public const string Risk = "Projects.Risk";
    }

    public static class Production
    {
        public const string View = "Production.View";
        public const string WorkOrders = "Production.WorkOrders";
        public const string CreateWorkOrder = "Production.CreateWorkOrder";
        public const string EditWorkOrder = "Production.EditWorkOrder";
        public const string IssueMaterial = "Production.IssueMaterial";
        public const string Start = "Production.Start";
        public const string Complete = "Production.Complete";
        public const string Schedule = "Production.Schedule";
    }

    public static class Procurement
    {
        public const string View = "Procurement.View";
        public const string CreatePO = "Procurement.CreatePO";
        public const string EditPO = "Procurement.EditPO";
        public const string ApprovePO = "Procurement.ApprovePO";
        public const string ReceiveGoods = "Procurement.ReceiveGoods";
        public const string Export = "Procurement.Export";
    }

    public static class Quality
    {
        public const string View = "Quality.View";
        public const string WPS = "Quality.WPS";
        public const string PQR = "Quality.PQR";
        public const string WeldRegister = "Quality.WeldRegister";
        public const string Repairs = "Quality.Repairs";
        public const string NCR = "Quality.NCR";
        public const string NcrDisposition = "Quality.NCR.Disposition";
        public const string NcrVerify = "Quality.NCR.Verify";
        public const string NcrClose = "Quality.NCR.Close";
        public const string NDT = "Quality.NDT";
        public const string HoldPointApproval = "Quality.HoldPointApproval";
        public const string Export = "Quality.Export";
    }

    public static class Administration
    {
        public const string Users = "Administration.Users";
        public const string AuditLog = "Administration.AuditLog";
        public const string Security = "Administration.Security";
        public const string Settings = "Administration.Settings";
    }

    public static class Reports
    {
        public const string View = "Reports.View";
        public const string Export = "Reports.Export";
    }
}
