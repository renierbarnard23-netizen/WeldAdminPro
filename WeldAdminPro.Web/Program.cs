using MudBlazor.Services;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.Data.Services.Inventory;
using WeldAdminPro.Data.Services.Procurement;
using WeldAdminPro.Data.Services.Production;
using WeldAdminPro.Data.Services.ProductionEngine;
using WeldAdminPro.Data.Services.Projects;
using WeldAdminPro.Data.Services.Quality;
using WeldAdminPro.Web.Components;
using WeldAdminPro.Web.Services.Dashboard;
using WeldAdminPro.Web.Services.Quality;
using WeldAdminPro.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Razor Components
// --------------------------------------------------

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// --------------------------------------------------
// WeldAdmin Pro Services
// --------------------------------------------------

builder.Services.AddSingleton<ProductionEngineService>();

builder.Services.AddScoped<DashboardService>();

builder.Services.AddScoped<ProductionApplicationService>();

builder.Services.AddScoped<ProjectRiskService>();

builder.Services.AddScoped<ProjectCostingService>();

builder.Services.AddScoped<MaterialTrendService>();

builder.Services.AddScoped<StockForecastService>();

builder.Services.AddScoped<MaterialDemandForecastService>();

builder.Services.AddSingleton<ProjectApplicationService>();

builder.Services.AddScoped<WorkOrderRepository>();

// Register the concrete repository
builder.Services.AddScoped<WeldRepository>();

// Register the interface using the same instance
builder.Services.AddScoped<IWeldRepository>(sp =>
    sp.GetRequiredService<WeldRepository>());

builder.Services.AddScoped<WeldTraceabilityRepository>();

builder.Services.AddScoped<WeldHistoryRepository>();

builder.Services.AddScoped<WorkOrderShortageDetectionService>();

builder.Services.AddScoped<ProductionReadinessService>();

builder.Services.AddSingleton<StockApplicationService>();

builder.Services.AddScoped<PurchaseOrderApplicationService>();

builder.Services.AddMudServices();

builder.Services.AddScoped<WorkOrderExecutionService>();

builder.Services.AddScoped<WorkOrderMaterialRepository>();

builder.Services.AddScoped<StockRepository>();

builder.Services.AddScoped<StockTransactionRepository>();

builder.Services.AddScoped<ProjectStockUsageRepository>();

builder.Services.AddScoped<BillOfMaterialRepository>();

builder.Services.AddScoped<MaterialValidator>();

builder.Services.AddScoped<ProjectRepository>();

builder.Services.AddScoped<StockProjectTransactionService>();

builder.Services.AddScoped<SmartPurchaseOrderService>();

builder.Services.AddScoped<QualityDashboardService>();

builder.Services.AddScoped<WpsApplicationService>();

builder.Services.AddScoped<WpsRepository>();

builder.Services.AddScoped<WelderQualificationRepository>();

builder.Services.AddSingleton<ProjectProfitabilityService>();

builder.Services.AddScoped<PersistentReservationService>();

builder.Services.AddScoped<SmartReorderPlannerService>();

builder.Services.AddScoped<InventoryRiskSummaryService>();

builder.Services.AddScoped<InventoryRiskService>();

builder.Services.AddScoped<InventoryAnomalyDetectionService>();

builder.Services.AddScoped<MaterialCostIntelligenceService>();

builder.Services.AddScoped<DocumentStorageService>();

builder.Services.AddScoped<WeldRegisterApplicationService>();

builder.Services.AddScoped<WeldTraceabilityApplicationService>();

builder.Services.AddScoped<RepairApplicationService>();

builder.Services.AddScoped<IWeldService, WeldService>();

// --------------------------------------------------

var app = builder.Build();

// --------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();