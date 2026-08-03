using WeldAdminPro.Core.Security;
using WeldAdminPro.Web.Models.Navigation;
using WeldAdminPro.Web.Services.Security;
using WeldAdminPro.Data.Services.Security;
using WeldAdminPro.Core.Security.Abstractions;

namespace WeldAdminPro.Web.Services.Navigation;

public class NavigationService
{

    private readonly ICurrentUserContext _userContext;
    private readonly PermissionAuthorizationService _permissionAuthorization;

    public NavigationService(
    ICurrentUserContext userContext,
    PermissionAuthorizationService permissionAuthorization)
    {
        _userContext = userContext;
        _permissionAuthorization = permissionAuthorization;
    }
    public async Task<IReadOnlyList<NavigationNode>> GetNavigationAsync()
    {
        List<NavigationNode> nodes =
        [
            // Dashboard
            new NavigationNode
            {
                Text = "Dashboard",
                Url = "/",
                Icon = "🏠"
            },

            // Projects
            new NavigationNode
            {
                Text = "Projects",
                Icon = "📁",
                Permission = PermissionKeys.Projects.View,

                Children =
                [
                    new()
                    {
                        Text = "Projects",
                        Url = "/projects"
                    },

                    new()
                    {
                        Text = "Project Costs",
                        Url = "/projects/costs"
                    },

                    new()
                    {
                        Text = "Project Profitability",
                        Url = "/projects/profitability"
                    },

                    new()
                    {
                        Text = "Project Risk",
                        Url = "/projects/risk"
                    },

                    new()
                    {
                        Text = "Project Compliance",
                        Url = "/projects/compliance",
                        Permission = PermissionKeys.Projects.Compliance
                    }
                ]
            },

            // Inventory
            new NavigationNode
            {
                Text = "Inventory",
                Icon = "📦",
                Permission = PermissionKeys.Inventory.View,

                Children =
                [
                    new()
                    {
                        Text = "Inventory Dashboard",
                        Url = "/inventory"
                    },

                    new()
                    {
                        Text = "Receive Stock",
                        Url = "/inventory/stockin",
                        Permission = PermissionKeys.Inventory.StockIn
                    },

                    new()
                    {
                        Text = "Issue Stock",
                        Url = "/inventory/stockout",
                        Permission = PermissionKeys.Inventory.StockOut
                    },

                    new()
                    {
                        Text = "Transactions",
                        Url = "/inventory/transactions"
                    },

                    new()
                    {
                        Text = "Ledger",
                        Url = "/inventory/ledger"
                    },

                    new()
                    {
                        Text = "Stock Forecast",
                        Url = "/inventory/forecast",
                        Permission = PermissionKeys.Inventory.Forecast
                    },

                    new()
                    {
                        Text = "Material Trends",
                        Url = "/inventory/material-trends"
                    },

                    new()
                    {
                        Text = "Project Risk",
                        Url = "/inventory/projectrisk"
                    },

                    new()
                    {
                        Text = "Smart Reorder Planner",
                        Url = "/inventory/smart-reorder"
                    },

                    new()
                    {
                        Text = "Material Cost Drivers",
                        Url = "/inventory/material-cost-drivers"
                    },
                ]
            },

            new NavigationNode
                    {
                        Text = "Production",
                        Icon = "🏭",
                        Permission = PermissionKeys.Production.View,

                        Children =
                        [
                            new()
                            {
                                Text = "Dashboard",
                                Url = "/production"
                            },

                            new()
                            {
                                Text = "Work Orders",
                                Url = "/production/workorders",
                                Permission = PermissionKeys.Production.WorkOrders
                            },

                            new()
                            {
                                Text = "Production Settings",
                                Url = "/production/settings"
                            }
                            ]
                    },

            new NavigationNode
            {
                Text = "Procurement",
                Icon = "🛒",
                Permission = PermissionKeys.Procurement.View,

                Children =
                [
                    new()
                    {
                        Text = "Purchase Orders",
                        Url = "/procurement/purchase-orders",
                        Permission = PermissionKeys.Procurement.CreatePO
                    }
                ]
            },

            new NavigationNode
            {
                Text = "Quality",
                Icon = "✔",
                Permission = PermissionKeys.Quality.View,

                Children =
                [
                    new()
                    {
                        Text = "Quality Dashboard",
                        Url = "/quality"
                    },

                    new()
                    {
                        Text = "Weld Register",
                        Url = "/quality/project-weld-register",
                        Permission = PermissionKeys.Quality.WeldRegister
                    },

                    new()
                    {
                        Text = "WPS",
                        Url = "/quality/wps",
                        Permission = PermissionKeys.Quality.WPS
                    },

                    new()
                    {
                        Text = "Welder Qualifications",
                        Url = "/quality/welders"
                    },

                    new()
                    {
                        Text = "Repairs",
                        Url = "/quality/repairs"
                    }
                ]
            },

            new NavigationNode
            {
                Text = "Reports",
                Icon = "📊",
                Url = "/reports"
            },

            new NavigationNode
            {
                Text = "Administration",
                Icon = "⚙",

                Children =
                [
                    new()
                    {
                        Text = "User Management",
                        Url = "/administration/users",
                        Permission = PermissionKeys.Administration.Users
                    },

                    new()
                    {
                        Text = "Audit Log",
                        Url = "/administration/audit",
                        Permission = PermissionKeys.Administration.AuditLog
                    },

                    new()
                    {
                        Text = "Production Settings",
                        Url = "/production/settings"
                    }
                ]
            }
        ];

        return await FilterNodesAsync(nodes);
    }

        private async Task<IReadOnlyList<NavigationNode>> FilterNodesAsync(
    IReadOnlyList<NavigationNode> nodes)
    {
        var result = new List<NavigationNode>();

        foreach (var node in nodes)
        {
            var filtered = await FilterNodeAsync(node);

            if (filtered != null)
                result.Add(filtered);
        }

        return result;
    }

    private async Task<NavigationNode?> FilterNodeAsync(
    NavigationNode node)
    {
        bool allowed = true;

        // --------------------------------------------------
        // Check this node's permission
        // --------------------------------------------------

        if (!string.IsNullOrWhiteSpace(node.Permission))
        {
            if (!_userContext.IsAuthenticated)
            {
                allowed = false;
            }
            else if (string.IsNullOrWhiteSpace(_userContext.Role))
            {
                allowed = false;
            }
            else
            {
                allowed =
                    await _permissionAuthorization.HasPermissionAsync(
                        _userContext.Role,
                        node.Permission);
            }
        }

        // --------------------------------------------------
        // Filter child nodes
        // --------------------------------------------------

        var copy = new NavigationNode
        {
            Text = node.Text,
            Url = node.Url,
            Icon = node.Icon,
            Permission = node.Permission,
            Expanded = node.Expanded
        };

        foreach (var child in node.Children)
        {
            var filteredChild =
                await FilterNodeAsync(child);

            if (filteredChild != null)
            {
                copy.Children.Add(filteredChild);
            }
        }

        // --------------------------------------------------
        // Hide node when:
        // - its own permission failed
        // - and it has no visible children
        // --------------------------------------------------

        if (!allowed && copy.Children.Count == 0)
        {
            return null;
        }

        return copy;
    }
}
