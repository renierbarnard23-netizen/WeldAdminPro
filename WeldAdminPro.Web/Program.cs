using WeldAdminPro.Web.Components;

using WeldAdminPro.Data.Services.ProductionEngine;
using WeldAdminPro.Data.Services;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Razor Components
// --------------------------------------------------

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// --------------------------------------------------
// Enterprise Services
// --------------------------------------------------

builder.Services.AddSingleton<ProductionEngineService>();

// --------------------------------------------------

var app = builder.Build();

// --------------------------------------------------
// Configure HTTP Pipeline
// --------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

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