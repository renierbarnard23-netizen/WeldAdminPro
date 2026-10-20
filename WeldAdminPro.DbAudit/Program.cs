using Microsoft.Data.Sqlite;

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "WeldAdminPro",
    "weldadmin.db");

Console.WriteLine();
Console.WriteLine("==================================================");
Console.WriteLine(" WELDADMIN PRO - C# DATABASE PERMISSION AUDIT");
Console.WriteLine(" READ-ONLY");
Console.WriteLine("==================================================");
Console.WriteLine();

Console.WriteLine($"Database: {dbPath}");

if (!File.Exists(dbPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("DATABASE NOT FOUND");
    Console.ResetColor();
    return;
}

Console.WriteLine("Database found.");
Console.WriteLine();

var connectionString =
    new SqliteConnectionStringBuilder
    {
        DataSource = dbPath,
        Mode = SqliteOpenMode.ReadOnly
    }.ToString();

using var connection = new SqliteConnection(connectionString);

connection.Open();

static void RunQuery(
    SqliteConnection connection,
    string title,
    string sql)
{
    Console.WriteLine();
    Console.WriteLine("==================================================");
    Console.WriteLine($" {title}");
    Console.WriteLine("==================================================");

    using var command = connection.CreateCommand();
    command.CommandText = sql;

    using var reader = command.ExecuteReader();

    if (!reader.HasRows)
    {
        Console.WriteLine("NO RESULTS");
        return;
    }

    while (reader.Read())
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            Console.Write($"{reader.GetName(i)}: ");

            if (reader.IsDBNull(i))
                Console.WriteLine("<NULL>");
            else
                Console.WriteLine(reader.GetValue(i));
        }

        Console.WriteLine("------------------------------------------");
    }
}

RunQuery(
    connection,
    "DATABASE TABLES",
    """
    SELECT name
    FROM sqlite_master
    WHERE type = 'table'
    ORDER BY name;
    """);

RunQuery(
    connection,
    "DATABASE SCHEMA VERSION",
    """
    SELECT
        SchemaVersion,
        BuildVersion,
        AppliedDate,
        Notes
    FROM DatabaseVersions
    ORDER BY SchemaVersion;
    """);
RunQuery(
    connection,
    "USERS AND ROLES",
    """
    SELECT
        u.Username,
        u.RoleId,
        r.Name AS RoleName,
        u.IsActive
    FROM SystemUsers u
    LEFT JOIN Roles r ON r.Id = u.RoleId
    ORDER BY u.Username;
    """);

RunQuery(
    connection,
    "ROLES WITH CREATE WORK ORDER",
    """
    SELECT
        r.Name AS RoleName,
        p.[Key] AS PermissionKey
    FROM Roles r
    JOIN RolePermissions rp
        ON rp.RoleId = r.Id
    JOIN Permissions p
        ON p.Id = rp.PermissionId
    WHERE p.[Key] = 'Production.CreateWorkOrder'
    ORDER BY r.Name;
    """);

RunQuery(
    connection,
    "USER-SPECIFIC CREATE WORK ORDER OVERRIDES",
    """
    SELECT
        u.Username,
        p.[Key] AS PermissionKey,
        up.IsGranted
    FROM UserPermissions up
    JOIN SystemUsers u
        ON u.Id = up.UserId
    JOIN Permissions p
        ON p.Id = up.PermissionId
    WHERE p.[Key] = 'Production.CreateWorkOrder';
    """);

Console.WriteLine();
Console.WriteLine("==================================================");
Console.WriteLine(" DATABASE AUDIT COMPLETE");
Console.WriteLine(" DATABASE WAS OPENED READ-ONLY");
Console.WriteLine("==================================================");
