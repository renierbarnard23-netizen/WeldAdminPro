using WeldAdminPro.Blazor;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register repositories
builder.Services.AddScoped<ProjectRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// .NET 8 Blazor routing
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
