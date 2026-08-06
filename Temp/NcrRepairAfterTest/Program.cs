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
    " NCR-009 -> REP-0001 - AFTER VERIFICATION");
Console.WriteLine(
    "==================================================");

Console.WriteLine();
Console.WriteLine(
    $"Database: {db}");

if (!File.Exists(db))
{
    throw new Exception(
        $"Live database not found: {db}");
}

using var connection =
    new SqliteConnection(
        $"Data Source={db};Mode=ReadOnly");

connection.Open();

Console.WriteLine();
Console.WriteLine(
    "PASS: Live database opened READ-ONLY.");

// ==================================================
// NCR-009
// ==================================================

Console.WriteLine();
Console.WriteLine(
    "========== NCR-009 AFTER START REPAIR ==========");

string ncrId;
string weldId;
long ncrStatus;

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

    ncrStatus =
        reader.GetInt64(
            reader.GetOrdinal("Status"));
}

// ==================================================
// EXACT REPAIR LINKED TO NCR-009
// ==================================================

Console.WriteLine();
Console.WriteLine(
    "========== REPAIR LINKED TO NCR-009 ==========");

string? repairId = null;
string? repairNcrId = null;
string? repairWeldId = null;
long repairNumber = 0;
long repairStatus = -1;
string? reason = null;
string? authorizedBy = null;
string? requestedDate = null;

var linkedCount = 0;

using (var cmd = connection.CreateCommand())
{
    cmd.CommandText =
        """
        SELECT
            Id,
            WeldId,
            RepairNumber,
            Reason,
            AuthorizedBy,
            RequestedDate,
            AuthorizedDate,
            ExcavationMethod,
            RepairWpsNumber,
            RepairedByWelder,
            ReinspectionResult,
            Notes,
            Status,
            CompletedDate,
            ApprovedBy,
            ApprovedDate,
            NcrId
        FROM RepairRecords
        WHERE NcrId = $ncrId
        ORDER BY RepairNumber;
        """;

    cmd.Parameters.AddWithValue(
        "$ncrId",
        ncrId);

    using var reader =
        cmd.ExecuteReader();

    while (reader.Read())
    {
        linkedCount++;

        Console.WriteLine();
        Console.WriteLine(
            $"Linked Repair #{linkedCount}");

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
                $"  {reader.GetName(i),-25}: {value}");
        }

        if (linkedCount == 1)
        {
            repairId =
                reader.GetString(
                    reader.GetOrdinal("Id"));

            repairWeldId =
                reader.GetString(
                    reader.GetOrdinal("WeldId"));

            repairNumber =
                reader.GetInt64(
                    reader.GetOrdinal("RepairNumber"));

            reason =
                reader.IsDBNull(
                    reader.GetOrdinal("Reason"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("Reason"));

            authorizedBy =
                reader.IsDBNull(
                    reader.GetOrdinal("AuthorizedBy"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("AuthorizedBy"));

            requestedDate =
                reader.IsDBNull(
                    reader.GetOrdinal("RequestedDate"))
                    ? null
                    : Convert.ToString(
                        reader.GetValue(
                            reader.GetOrdinal("RequestedDate")));

            repairStatus =
                reader.GetInt64(
                    reader.GetOrdinal("Status"));

            repairNcrId =
                reader.IsDBNull(
                    reader.GetOrdinal("NcrId"))
                    ? null
                    : reader.GetString(
                        reader.GetOrdinal("NcrId"));
        }
    }
}

// ==================================================
// GLOBAL COUNTS
// ==================================================

Console.WriteLine();
Console.WriteLine(
    "========== REPAIR COUNTS ==========");

long totalRepairs;
long weldRepairs;

using (var cmd = connection.CreateCommand())
{
    cmd.CommandText =
        "SELECT COUNT(*) FROM RepairRecords;";

    totalRepairs =
        Convert.ToInt64(
            cmd.ExecuteScalar());
}

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
}

Console.WriteLine(
    $"Repairs linked to NCR-009 : {linkedCount}");

Console.WriteLine(
    $"Total RepairRecords       : {totalRepairs}");

Console.WriteLine(
    $"RepairRecords for W-001   : {weldRepairs}");

// ==================================================
// TRACEABILITY ASSERTIONS
// ==================================================

Console.WriteLine();
Console.WriteLine(
    "========== TRACEABILITY ASSERTIONS ==========");

var pass = true;

if (linkedCount == 1)
{
    Console.WriteLine(
        "PASS: Exactly one RepairRecord is linked to NCR-009.");
}
else
{
    Console.WriteLine(
        $"FAIL: Expected 1 linked repair, found {linkedCount}.");

    pass = false;
}

if (totalRepairs == 1)
{
    Console.WriteLine(
        "PASS: RepairRecords increased from 0 to exactly 1.");
}
else
{
    Console.WriteLine(
        $"FAIL: Expected total RepairRecords = 1, found {totalRepairs}.");

    pass = false;
}

if (weldRepairs == 1)
{
    Console.WriteLine(
        "PASS: W-001 has exactly one RepairRecord.");
}
else
{
    Console.WriteLine(
        $"FAIL: Expected one repair for W-001, found {weldRepairs}.");

    pass = false;
}

if (repairNcrId == ncrId)
{
    Console.WriteLine(
        "PASS: RepairRecord.NcrId exactly matches NCR-009.Id.");
}
else
{
    Console.WriteLine(
        "FAIL: RepairRecord.NcrId does not match NCR-009.Id.");

    pass = false;
}

if (repairWeldId == weldId)
{
    Console.WriteLine(
        "PASS: RepairRecord.WeldId exactly matches NCR-009.WeldId.");
}
else
{
    Console.WriteLine(
        "FAIL: RepairRecord.WeldId does not match NCR-009.WeldId.");

    pass = false;
}

if (repairNumber == 1)
{
    Console.WriteLine(
        "PASS: First repair received RepairNumber 1 (REP-0001).");
}
else
{
    Console.WriteLine(
        $"FAIL: Expected RepairNumber 1, found {repairNumber}.");

    pass = false;
}

if (!string.IsNullOrWhiteSpace(reason) &&
    reason.Contains(
        "NCR-009",
        StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine(
        "PASS: Repair reason carries NCR-009 traceability.");
}
else
{
    Console.WriteLine(
        "FAIL: Repair reason does not reference NCR-009.");

    pass = false;
}

if (string.Equals(
        authorizedBy,
        "Renier",
        StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine(
        "PASS: Repair AuthorizedBy = Renier.");
}
else
{
    Console.WriteLine(
        $"FAIL: Repair AuthorizedBy = {authorizedBy ?? "NULL"}.");

    pass = false;
}

if (!string.IsNullOrWhiteSpace(requestedDate))
{
    Console.WriteLine(
        "PASS: Repair RequestedDate is populated.");
}
else
{
    Console.WriteLine(
        "FAIL: Repair RequestedDate is empty.");

    pass = false;
}

Console.WriteLine();
Console.WriteLine(
    "========== RAW STATUS VALUES ==========");

Console.WriteLine(
    $"NCR Status    : {ncrStatus}");

Console.WriteLine(
    $"Repair Status : {repairStatus}");

Console.WriteLine();
Console.WriteLine(
    "========== IDENTIFIERS ==========");

Console.WriteLine(
    $"NCR Id        : {ncrId}");

Console.WriteLine(
    $"Repair Id     : {repairId ?? "NULL"}");

Console.WriteLine(
    $"NCR WeldId    : {weldId}");

Console.WriteLine(
    $"Repair WeldId : {repairWeldId ?? "NULL"}");

Console.WriteLine(
    $"Repair NcrId  : {repairNcrId ?? "NULL"}");

Console.WriteLine();
Console.WriteLine(
    "==================================================");

if (pass)
{
    Console.WriteLine(
        " NCR -> REPAIR TRACEABILITY TEST PASSED");
}
else
{
    Console.WriteLine(
        " NCR -> REPAIR TRACEABILITY TEST FAILED");
}

Console.WriteLine(
    "==================================================");

if (!pass)
{
    Environment.ExitCode = 1;
}
