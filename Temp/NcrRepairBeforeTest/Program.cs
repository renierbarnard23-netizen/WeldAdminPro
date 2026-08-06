using Microsoft.Data.Sqlite;

var db =
    Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData),
        "WeldAdminPro",
        "weldadmin.db");

Console.WriteLine();
Console.WriteLine(
    "==================================================");
Console.WriteLine(
    " NCR-009 -> REPAIR - FINAL BEFORE CHECK");
Console.WriteLine(
    "==================================================");

Console.WriteLine();
Console.WriteLine(
    $"Database: {db}");

using var connection =
    new SqliteConnection(
        $"Data Source={db};Mode=ReadOnly");

connection.Open();

Console.WriteLine();
Console.WriteLine(
    "PASS: Live database opened READ-ONLY.");

// ==================================================
// REPAIRRECORDS ACTUAL SCHEMA
// ==================================================

Console.WriteLine();
Console.WriteLine(
    "========== REPAIRRECORDS ACTUAL SCHEMA ==========");

using (var cmd = connection.CreateCommand())
{
    cmd.CommandText =
        "PRAGMA table_info(RepairRecords);";

    using var reader =
        cmd.ExecuteReader();

    while (reader.Read())
    {
        Console.WriteLine(
            $"  {reader.GetString(1),-25} {reader.GetString(2)}");
    }
}

// ==================================================
// NCR-009
// ==================================================

Console.WriteLine();
Console.WriteLine(
    "========== NCR-009 BEFORE START REPAIR ==========");

string ncrId;
string weldId;
long status;
long disposition;

using (var cmd = connection.CreateCommand())
{
    cmd.CommandText =
        """
        SELECT
            Id,
            NcrNumber,
            WeldId,
            WeldNumber,
            Status,
            IsClosed,
            DispositionType,
            Description,
            RootCause,
            CorrectiveAction,
            PreventiveAction,
            DispositionApprovedBy,
            DispositionApprovedDate
        FROM NcrRecords
        WHERE NcrNumber = 'NCR-009';
        """;

    using var reader =
        cmd.ExecuteReader();

    if (!reader.Read())
    {
        throw new Exception(
            "NCR-009 not found.");
    }

    for (var i = 0;
         i < reader.FieldCount;
         i++)
    {
        var value =
            reader.IsDBNull(i)
                ? "NULL"
                : Convert.ToString(
                    reader.GetValue(i))
                  ?? "";

        Console.WriteLine(
            $"  {reader.GetName(i),-28}: {value}");
    }

    ncrId =
        reader.GetString(
            reader.GetOrdinal("Id"));

    weldId =
        reader.GetString(
            reader.GetOrdinal("WeldId"));

    status =
        reader.GetInt64(
            reader.GetOrdinal("Status"));

    disposition =
        reader.GetInt64(
            reader.GetOrdinal("DispositionType"));
}

// ==================================================
// LINKED REPAIR COUNT
// ==================================================

Console.WriteLine();
Console.WriteLine(
    "========== LINKED REPAIR CHECK ==========");

long linkedRepairs;

using (var cmd = connection.CreateCommand())
{
    cmd.CommandText =
        """
        SELECT COUNT(*)
        FROM RepairRecords
        WHERE NcrId = $ncrId;
        """;

    cmd.Parameters.AddWithValue(
        "$ncrId",
        ncrId);

    linkedRepairs =
        Convert.ToInt64(
            cmd.ExecuteScalar());

    Console.WriteLine(
        $"NCR-009 linked repairs : {linkedRepairs}");
}

// ==================================================
// TOTAL REPAIR COUNT
// ==================================================

long totalRepairs;

using (var cmd = connection.CreateCommand())
{
    cmd.CommandText =
        """
        SELECT COUNT(*)
        FROM RepairRecords;
        """;

    totalRepairs =
        Convert.ToInt64(
            cmd.ExecuteScalar());

    Console.WriteLine(
        $"Total RepairRecords    : {totalRepairs}");
}

// ==================================================
// SAME-WELD COUNT
// ==================================================

long weldRepairs;

using (var cmd = connection.CreateCommand())
{
    cmd.CommandText =
        """
        SELECT COUNT(*)
        FROM RepairRecords
        WHERE WeldId = $weldId;
        """;

    cmd.Parameters.AddWithValue(
        "$weldId",
        weldId);

    weldRepairs =
        Convert.ToInt64(
            cmd.ExecuteScalar());

    Console.WriteLine(
        $"Repairs for W-001      : {weldRepairs}");
}

// ==================================================
// ASSERTIONS
// ==================================================

Console.WriteLine();
Console.WriteLine(
    "========== FINAL BEFORE ASSERTIONS ==========");

var pass = true;

if (linkedRepairs == 0)
{
    Console.WriteLine(
        "PASS: NCR-009 has no RepairRecord yet.");
}
else
{
    Console.WriteLine(
        "FAIL: NCR-009 already has a RepairRecord.");

    pass = false;
}

if (totalRepairs == 0)
{
    Console.WriteLine(
        "PASS: RepairRecords table contains zero records.");
}
else
{
    Console.WriteLine(
        $"FAIL: RepairRecords already contains {totalRepairs} record(s).");

    pass = false;
}

if (weldRepairs == 0)
{
    Console.WriteLine(
        "PASS: W-001 has no repair records yet.");
}
else
{
    Console.WriteLine(
        $"FAIL: W-001 already has {weldRepairs} repair record(s).");

    pass = false;
}

if (!string.IsNullOrWhiteSpace(ncrId))
{
    Console.WriteLine(
        $"PASS: NCR Id captured: {ncrId}");
}
else
{
    Console.WriteLine(
        "FAIL: NCR Id is empty.");

    pass = false;
}

if (!string.IsNullOrWhiteSpace(weldId))
{
    Console.WriteLine(
        $"PASS: Weld Id captured: {weldId}");
}
else
{
    Console.WriteLine(
        "FAIL: Weld Id is empty.");

    pass = false;
}

Console.WriteLine();
Console.WriteLine(
    $"Raw NCR Status      : {status}");

Console.WriteLine(
    $"Raw DispositionType : {disposition}");

Console.WriteLine();
Console.WriteLine(
    "==================================================");

if (pass)
{
    Console.WriteLine(
        " FINAL BEFORE CHECK PASSED");
}
else
{
    Console.WriteLine(
        " FINAL BEFORE CHECK FAILED");
}

Console.WriteLine(
    "==================================================");

if (!pass)
{
    Environment.ExitCode = 1;
}
