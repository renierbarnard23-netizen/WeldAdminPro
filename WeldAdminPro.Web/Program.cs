using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Repositories.Security;
using WeldAdminPro.Data.Services;
using WeldAdminPro.Data.Services.Inventory;
using WeldAdminPro.Data.Services.Procurement;
using WeldAdminPro.Data.Services.Production;
using WeldAdminPro.Data.Services.ProductionEngine;
using WeldAdminPro.Data.Services.Projects;
using WeldAdminPro.Data.Services.Quality;
using WeldAdminPro.Data.Services.Recognition;
using WeldAdminPro.Web.Components;
using WeldAdminPro.Web.Security;
using WeldAdminPro.Web.Services.Dashboard;
using WeldAdminPro.Web.Services.Import;
using WeldAdminPro.Web.Services.Navigation;
using WeldAdminPro.Web.Services.Quality;
using WeldAdminPro.Web.Services.Security;
using WeldAdminPro.Data.Services.Security;
using static WeldAdminPro.Web.Services.Quality.PqrApplicationService;
using Microsoft.AspNetCore.Authorization;
using WeldAdminPro.Web.Security.Authorization;
using WeldAdminPro.Core.Security.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Razor Components
// --------------------------------------------------

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

// --------------------------------------------------
// Authentication
// --------------------------------------------------

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";

        options.Cookie.Name = "WeldAdminPro.Auth";

        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                Console.WriteLine(
                    $"*** RedirectToLogin: {context.Request.Path} -> {context.RedirectUri}");

                context.Response.Redirect(context.RedirectUri);

                return Task.CompletedTask;
            },

            OnRedirectToAccessDenied = context =>
            {
                Console.WriteLine(
                    $"*** RedirectToAccessDenied: {context.Request.Path} -> {context.RedirectUri}");

                context.Response.Redirect(context.RedirectUri);

                return Task.CompletedTask;
            },

            OnValidatePrincipal = context =>
            {
                Console.WriteLine(
                    $"*** Authenticated: {context.Principal?.Identity?.IsAuthenticated}");

                Console.WriteLine(
                    $"*** User: {context.Principal?.Identity?.Name}");

                var role =
                    context.Principal?
                        .FindFirst(
                            System.Security.Claims.ClaimTypes.Role)?
                        .Value;

                Console.WriteLine($"*** Role: {role}");

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IAuthorizationPolicyProvider,
    PermissionPolicyProvider>();

builder.Services.AddScoped<IAuthorizationHandler,
    PermissionAuthorizationHandler>();

builder.Services.AddCascadingAuthenticationState();

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

builder.Services.AddScoped<ProjectApplicationService>();

builder.Services.AddScoped<AdministrationApplicationService>();

builder.Services.AddScoped<WorkOrderRepository>();

// Register the concrete repository
builder.Services.AddScoped<WeldRepository>();

// Register the interface using the same instance
builder.Services.AddScoped<IWeldRepository>(sp =>
    sp.GetRequiredService<WeldRepository>());

builder.Services.AddScoped<SystemUserRepository>(sp =>
    new SystemUserRepository(
        $"Data Source={DatabasePath.Get()}"));

builder.Services.AddScoped<AuthenticationService>();

builder.Services.AddScoped<WeldTraceabilityRepository>();

builder.Services.AddScoped<WeldHistoryRepository>();

builder.Services.AddScoped<WorkOrderShortageDetectionService>();

builder.Services.AddScoped<ProductionReadinessService>();

builder.Services.AddScoped<StockApplicationService>();

builder.Services.AddScoped<AuditService>();

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

builder.Services.AddScoped<PqrApplicationService>();

builder.Services.AddScoped<PqrParserService>();

builder.Services.AddScoped<PqrOcrService>();

builder.Services.AddSingleton<MaterialLibraryService>();

builder.Services.AddScoped<MaterialSearchService>();

builder.Services.AddSingleton<FillerMaterialSearchService>();

builder.Services.AddScoped<PdfToImageService>();

builder.Services.AddScoped<WpsOcrService>();

builder.Services.AddScoped<WpsParserService>();

builder.Services.AddScoped<WpsRepository>();

builder.Services.AddScoped<PqrRepository>();

builder.Services.AddScoped<WelderQualificationRepository>();

builder.Services.AddScoped<WeldNdtRepository>();

builder.Services.AddScoped<ProjectDocumentRepository>();
builder.Services.AddScoped<NcrRepository>(_ =>
    new NcrRepository(
        DatabasePath.GetConnectionString()));

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

builder.Services.AddSingleton<TesseractService>();

builder.Services.AddScoped<PqrOcrService>();

builder.Services.AddScoped<PqrParserService>();

builder.Services.AddScoped<WpsOcrService>();

builder.Services.AddScoped<WpsParserService>();

if (OperatingSystem.IsWindows())
{
    builder.Services.AddScoped<IDocumentImporter, DocumentImportService>();
}

builder.Services.AddScoped<MaterialRecognitionService>();

builder.Services.AddScoped<TextNormalizationService>();

builder.Services.AddScoped<MaterialSectionExtractor>();

builder.Services.AddScoped<MaterialScoringService>();

builder.Services.AddScoped<PNumberRecognitionService>();

builder.Services.AddScoped<FNumberRecognitionService>();

builder.Services.AddScoped<ThicknessRecognitionService>();

builder.Services.AddSingleton<SmartMaterialExtractor>();

builder.Services.AddScoped<RecognitionEngine>();

builder.Services.AddScoped<SpecificationScanner>();

builder.Services.AddScoped<PqrNumberRecognitionService>();

builder.Services.AddScoped<HeaderRecognitionService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<NavigationService>();

builder.Services.AddScoped<WelderQualificationApplicationService>();

builder.Services.AddScoped<NdtApplicationService>();

builder.Services.AddScoped<DocumentApplicationService>();

builder.Services.AddScoped<NcrApplicationService>();

builder.Services.AddScoped<QualityComplianceService>();

builder.Services.AddScoped<QualityAlertService>();

builder.Services.AddScoped<QualityRecommendationService>();

builder.Services.AddScoped<QualityActivityService>();

builder.Services.AddSingleton(
    new RoleRepository(DatabasePath.GetConnectionString()));

builder.Services.AddSingleton(
    new PermissionRepository(DatabasePath.GetConnectionString()));

builder.Services.AddSingleton(
    new RolePermissionRepository(DatabasePath.GetConnectionString()));

builder.Services.AddSingleton(
    new UserPermissionRepository(DatabasePath.GetConnectionString()));

builder.Services.AddScoped<UserContextService>();

builder.Services.AddScoped<ICurrentUserContext>(
    sp => sp.GetRequiredService<UserContextService>());

builder.Services.AddScoped<
    WeldAdminPro.Data.Services.Security.PermissionAuthorizationService>();

builder.Services.AddScoped<
    WeldAdminPro.Core.Security.Abstractions.IPermissionAuthorizationService>(
        sp => sp.GetRequiredService<
            WeldAdminPro.Data.Services.Security.PermissionAuthorizationService>());

builder.Services.AddScoped<
    WeldAdminPro.Core.Quality.Services.HoldPointWorkflowService>();

// --------------------------------------------------

var app = builder.Build();

// --------------------------------------------------
// Initialize WeldAdmin Pro Database
// --------------------------------------------------

DatabaseInitializer.Initialize();

// --------------------------------------------------

var migrationService =
    new DatabaseMigrationService(
        DatabasePath.GetConnectionString());

migrationService.ApplyMigrations();

// --------------------------------------------------
// Seed default administrator after security migrations
// --------------------------------------------------

var defaultAdminSeeder =
    new WeldAdminPro.Data.Services.Security.DefaultAdminSeeder(
        DatabasePath.GetConnectionString());

await defaultAdminSeeder.SeedAsync();

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

app.UseAuthentication();

app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
