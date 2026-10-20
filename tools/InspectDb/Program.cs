using System;
using Microsoft.Data.Sqlite;

class Program
{
    static void Main(string[] args)
    {
        var dbPath = args.Length > 0 ? args[0] : "weldadmin.db";

        if (!System.IO.File.Exists(dbPath))
        {
            Console.WriteLine($"File not found: {dbPath}");
            return;
        }

        var connString = $"Data Source={dbPath}";

        using var conn = new SqliteConnection(connString);
        conn.Open();

        Console.WriteLine($"Database: {dbPath}");
        Console.WriteLine();

        // ============================================================
        // CURRENT SCHEMA VERSION
        // ============================================================

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
SELECT COALESCE(MAX(SchemaVersion), 0)
FROM DatabaseVersions;";

            var version = Convert.ToInt32(cmd.ExecuteScalar());

            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("CURRENT SCHEMA VERSION");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine($"SchemaVersion: {version}");
            Console.WriteLine();
        }

        // ============================================================
        // VERSION 19 TABLES
        // ============================================================

        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine("VERSION 19 TABLES");
        Console.WriteLine("------------------------------------------------------------");

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
SELECT name
FROM sqlite_master
WHERE type = 'table'
  AND name IN ('UserCompanyAccess', 'ProjectUserAssignment')
ORDER BY name;";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine(reader.GetString(0));
            }
        }

        Console.WriteLine();

        // ============================================================
        // TABLE COUNTS
        // ============================================================

        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine("TABLE COUNTS");
        Console.WriteLine("------------------------------------------------------------");

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM UserCompanyAccess;";
            Console.WriteLine($"UserCompanyAccess      : {cmd.ExecuteScalar()}");
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM ProjectUserAssignment;";
            Console.WriteLine($"ProjectUserAssignment  : {cmd.ExecuteScalar()}");
        }

        Console.WriteLine();

        // ============================================================
        // HOME-COMPANY ACCESS
        // ============================================================

        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine("HOME-COMPANY ACCESS");
        Console.WriteLine("------------------------------------------------------------");

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
SELECT
    u.Username,
    u.CompanyId AS HomeCompanyId,
    a.CompanyId AS AccessCompanyId,
    a.IsActive,
    a.CreatedBy,
    a.Id AS AccessId
FROM SystemUsers u
LEFT JOIN UserCompanyAccess a
    ON a.UserId = u.Id
   AND a.CompanyId = u.CompanyId
WHERE u.CompanyId IS NOT NULL
ORDER BY u.Username;";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine(
                    $"User={reader["Username"]}, " +
                    $"HomeCompany={reader["HomeCompanyId"]}, " +
                    $"AccessCompany={reader["AccessCompanyId"]}, " +
                    $"Active={reader["IsActive"]}, " +
                    $"CreatedBy={reader["CreatedBy"]}, " +
                    $"AccessId={reader["AccessId"]}");
            }
        }

        Console.WriteLine();

        // ============================================================
        // MISSING HOME-COMPANY ACCESS
        // ============================================================

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
SELECT COUNT(*)
FROM SystemUsers u
WHERE u.CompanyId IS NOT NULL
  AND NOT EXISTS
  (
      SELECT 1
      FROM UserCompanyAccess a
      WHERE a.UserId = u.Id
        AND a.CompanyId = u.CompanyId
        AND a.IsActive = 1
  );";

            var missing = Convert.ToInt32(cmd.ExecuteScalar());

            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("MISSING HOME-COMPANY ACCESS");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine($"Missing active home-company relationships: {missing}");
            Console.WriteLine();
        }

        // ============================================================
        // GUID FORMAT
        // ============================================================

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
SELECT COUNT(*)
FROM UserCompanyAccess
WHERE length(Id) <> 36;";

            var badGuidCount = Convert.ToInt32(cmd.ExecuteScalar());

            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("GUID FORMAT CHECK");
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine($"Non-canonical access IDs: {badGuidCount}");
            Console.WriteLine();
        }

        // ============================================================
        // VERSION 19 RECORD
        // ============================================================

        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine("VERSION 19 RECORD");
        Console.WriteLine("------------------------------------------------------------");

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
SELECT
    SchemaVersion,
    BuildVersion,
    AppliedDate,
    Notes
FROM DatabaseVersions
WHERE SchemaVersion = 19;";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine(
                    $"SchemaVersion={reader["SchemaVersion"]}, " +
                    $"BuildVersion={reader["BuildVersion"]}, " +
                    $"AppliedDate={reader["AppliedDate"]}, " +
                    $"Notes={reader["Notes"]}");
            }
        }

        Console.WriteLine();

        // ============================================================
        // PROJECT ASSIGNMENTS
        // ============================================================

        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine("PROJECT ASSIGNMENTS");
        Console.WriteLine("------------------------------------------------------------");

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
SELECT
    Id,
    ProjectId,
    UserId,
    RoleId,
    IsActive
FROM ProjectUserAssignment
ORDER BY ProjectId, UserId;";

            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                Console.WriteLine("No project assignments currently exist.");
            }
            else
            {
                while (reader.Read())
                {
                    Console.WriteLine(
                        $"Id={reader["Id"]}, " +
                        $"ProjectId={reader["ProjectId"]}, " +
                        $"UserId={reader["UserId"]}, " +
                        $"RoleId={reader["RoleId"]}, " +
                        $"Active={reader["IsActive"]}");
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine("============================================================");

        // ============================================================
        // SECURITYTEST PERMISSION AUDIT
        // ============================================================

        Console.WriteLine();
        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine("SECURITYTEST USER");
        Console.WriteLine("------------------------------------------------------------");

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
SELECT Username, Role, CompanyId, IsActive
FROM SystemUsers
WHERE lower(Username) = lower('securitytest');
";

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine(
                        $"User={reader["Username"]}, " +
                        $"Role={reader["Role"]}, " +
                        $"CompanyId={reader["CompanyId"]}, " +
                        $"Active={reader["IsActive"]}");
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine("------------------------------------------------------------");
        Console.WriteLine("SECURITYTEST WELD REGISTER PERMISSION");
        Console.WriteLine("------------------------------------------------------------");

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
SELECT COUNT(*)
FROM SystemUsers u
JOIN RolePermissions rp
    ON lower(rp.RoleName) = lower(u.Role)
JOIN Permissions p
    ON p.Id = rp.PermissionId
WHERE lower(u.Username) = lower('securitytest')
  AND p.PermissionKey = 'Quality.WeldRegister';
";

            var count = Convert.ToInt32(cmd.ExecuteScalar());

            Console.WriteLine(
                count > 0
                    ? "securitytest HAS Quality.WeldRegister"
                    : "securitytest DOES NOT HAVE Quality.WeldRegister");
        }

        Console.WriteLine("   VERSION 19 DATABASE AUDIT COMPLETE");
        Console.WriteLine("============================================================");
    }
}
