using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Security;
using WeldAdminPro.Core.Security.Abstractions;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Repositories.Security;
using WeldAdminPro.Data.Services;
using WeldAdminPro.Data.Services.Security;
using WeldAdminPro.Data.Services.Quality;

const string DatabasePath =
    @"C:\Users\renie\AppData\Roaming\WeldAdminPro\weldadmin.db";

const string ProjectId =
    "2cffe8c9-9195-4692-a569-96b13c75c8ec";

const string PlanId =
    "2bfd3c1f-ff0b-4056-93b1-967eb262208d";

const string ItemId =
    "8fa576cb-496f-4663-abd3-b86f8e552197";

const string WrongProjectHoldPointId =
    "4d2a6944-a396-4d40-9304-973445b2ac5e";

var connectionString = $"Data Source={DatabasePath}";

Console.WriteLine();
Console.WriteLine("============================================================");
Console.WriteLine(" Q3 ITP CROSS-PROJECT EXECUTION AUTHORIZATION TEST");
Console.WriteLine("============================================================");
Console.WriteLine();

using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();

    Console.WriteLine("--- Security Test Identity ---");

    using var command = connection.CreateCommand();

    command.CommandText = @"
        SELECT
            u.Id,
            u.Username,
            u.FullName,
            u.RoleId,
            r.Name AS RoleName,
            u.CompanyId
        FROM SystemUsers u
        INNER JOIN Roles r
            ON r.Id = u.RoleId
        WHERE u.Username = 'Quality'
          AND u.IsActive = 1;";

    using var reader = command.ExecuteReader();

    while (reader.Read())
    {
        Console.WriteLine($"User       : {reader["Username"]}");
        Console.WriteLine($"Full Name  : {reader["FullName"]}");
        Console.WriteLine($"Role       : {reader["RoleName"]}");
        Console.WriteLine($"Role ID    : {reader["RoleId"]}");
        Console.WriteLine($"Company ID : {reader["CompanyId"]}");
    }
}

var currentUser = new TestCurrentUserContext
{
    IsAuthenticated = true,
    UserId = "041e37d7-bb08-4adf-a30f-8b4c522d5280",
    Username = "Quality",
    FullName = "Quality Control",
    Role = "Quality Manager",
    CompanyId = Guid.Parse(
        "3ffcc2ca-5c28-4be7-9dd7-0f0ce256e641")
};

var projectRepository = new ProjectRepository();

var companyAccessRepository =
    new UserCompanyAccessRepository(connectionString);

var projectAssignmentRepository =
    new ProjectUserAssignmentRepository(connectionString);

var projectAccessAuthorization =
    new ProjectAccessAuthorizationService(
        currentUser,
        companyAccessRepository,
        projectAssignmentRepository);

var permissionAuthorization =
    new PermissionAuthorizationService(
        new RoleRepository(connectionString),
        new PermissionRepository(connectionString),
        new RolePermissionRepository(connectionString),
        new UserPermissionRepository(connectionString));

var itpService =
    new InspectionTestPlanApplicationService(
        new InspectionTestPlanRepository(connectionString),
        new InspectionTestPlanItemRepository(),
        new InspectionTestPlanExecutionRepository(),
        new ProjectQualityControlPlanRepository(connectionString),
        new ProjectQualityControlPlanItemRepository(),
        new CustomerQualityRequirementRepository(connectionString),
        projectRepository,
        new WeldRepository(),
        new HoldPointRepository(connectionString),
        new WeldNdtRepository(),
        currentUser,
        new AuditService(currentUser),
        permissionAuthorization,
        projectAccessAuthorization);

var plan = new InspectionTestPlanRepository(connectionString)
    .GetById(Guid.Parse(PlanId));

if (plan == null)
    throw new Exception("Target ITP was not found.");

var item = new InspectionTestPlanItemRepository()
    .GetById(Guid.Parse(ItemId));

if (item == null)
    throw new Exception("Target ITP item was not found.");

var wrongHoldPoint =
    new HoldPointRepository(connectionString)
        .GetById(Guid.Parse(WrongProjectHoldPointId));

if (wrongHoldPoint == null)
    throw new Exception("Wrong-project Hold Point was not found.");

var wrongWeld =
    await new WeldRepository()
        .GetByIdAsync(wrongHoldPoint.WeldId);

if (wrongWeld == null)
    throw new Exception("Wrong-project Hold Point weld was not found.");

var targetProject =
    projectRepository.GetById(Guid.Parse(ProjectId));

if (targetProject == null)
    throw new Exception("Target ITP project was not found.");

Console.WriteLine();
Console.WriteLine("--- Target ---");
Console.WriteLine($"ITP Project       : {targetProject.ProjectName}");
Console.WriteLine($"ITP Project ID    : {targetProject.Id}");
Console.WriteLine($"ITP Number        : {plan.ItpNumber}");
Console.WriteLine($"ITP Item          : {item.Activity}");

Console.WriteLine();
Console.WriteLine("--- Wrong-Project Execution Record ---");
Console.WriteLine($"Hold Point ID     : {wrongHoldPoint.Id}");
Console.WriteLine($"Weld ID           : {wrongWeld.Id}");
Console.WriteLine($"Wrong Project ID  : {wrongWeld.ProjectId}");

if (wrongWeld.ProjectId == Guid.Parse(ProjectId))
    throw new Exception(
        "TEST SETUP INVALID: wrong-project weld belongs to target project.");

var executionRepository =
    new InspectionTestPlanExecutionRepository();

var beforeCount =
    executionRepository.GetByItem(Guid.Parse(ItemId)).Count;

Console.WriteLine();
Console.WriteLine($"Executions before : {beforeCount}");

Console.WriteLine();
Console.WriteLine("Attempting cross-project execution link...");

var rejected = false;

try
{
    await itpService.LinkExecutionAsync(
        Guid.Parse(ItemId),
        InspectionTestPlanExecutionType.HoldPoint,
        WrongProjectHoldPointId,
        "Q3 cross-project authorization test");

    Console.WriteLine(
        "FAIL: LinkExecutionAsync unexpectedly accepted the wrong-project Hold Point.");
}
catch (UnauthorizedAccessException ex)
{
    rejected = true;

    Console.WriteLine(
        "PASS: LinkExecutionAsync rejected the wrong-project Hold Point.");

    Console.WriteLine(
        $"Message: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine(
        "FAIL: Unexpected exception type.");

    Console.WriteLine(
        $"Type: {ex.GetType().FullName}");

    Console.WriteLine(
        $"Message: {ex.Message}");
}

var afterCount =
    executionRepository.GetByItem(Guid.Parse(ItemId)).Count;

Console.WriteLine();
Console.WriteLine($"Executions after  : {afterCount}");

if (afterCount != beforeCount)
{
    Console.WriteLine(
        "FAIL: Execution count changed despite authorization rejection.");
}
else if (rejected)
{
    Console.WriteLine(
        "PASS: No execution was persisted.");
}

Console.WriteLine();

if (rejected && afterCount == beforeCount)
{
    Console.WriteLine(
        "============================================================");

    Console.WriteLine(
        "RESULT: PASS - CROSS-PROJECT EXECUTION LINK BLOCKED");

    Console.WriteLine(
        "============================================================");
}
else
{
    Console.WriteLine(
        "============================================================");

    Console.WriteLine(
        "RESULT: FAIL");

    Console.WriteLine(
        "============================================================");
}

public sealed class TestCurrentUserContext : ICurrentUserContext
{
    public bool IsAuthenticated { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public Guid? CompanyId { get; set; }

    public bool IsAdministrator =>
        string.Equals(
            Role,
            "Administrator",
            StringComparison.OrdinalIgnoreCase);
}


