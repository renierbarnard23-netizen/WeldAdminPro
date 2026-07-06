using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.Data.Services.Production;
using WeldAdminPro.Data.Services.ProductionEngine;
using WeldAdminPro.Web.Components;
using WeldAdminPro.Web.Services.Dashboard;

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

builder.Services.AddScoped<WorkOrderRepository>();

builder.Services.AddScoped<WorkOrderShortageDetectionService>();

builder.Services.AddScoped<ProductionReadinessService>();

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