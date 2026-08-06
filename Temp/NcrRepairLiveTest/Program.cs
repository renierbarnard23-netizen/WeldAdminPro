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
    " NCR -> REPAIR CORRECTED LIVE PRECHECK");
Console.WriteLine(
    "==================================================");

Console.WriteLine();
Console.WriteLine(
    $"Database: {db}");

if (!File.Exists(db))
{
    throw new Exception(
        $"Database not found: {db}");
}

using var connection =
    new SqliteConnection(
        $"Data Source={db};Mode=ReadOnly");

connection.Open();

Console.WriteLine();
Console.WriteLine(
    "PASS: Database opened READ-ONLY.");

// ==================================================
// NCR ENUM VALUE REFERENCE
// ==================================================

Console.WriteLine();
Console.WriteLine(
    "========== RAW DATABASE VALUES ==========");

Console.WriteLine(
    "Status and DispositionType below are stored enum integers.");
Console.WriteLine(
    "We will select the test NCR from the actual values shown.");

// ==================================================
// NCR RECORDS
// ==================================================

Console.WriteLine();
Console.WriteLine(
    "========== CURRENT NCR RECORDS ==========");

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
            CustomReason,
            RaisedBy,
            RaisedDate,
            DispositionApprovedBy,
            DispositionApprovedDate
        FROM NcrRecords
        ORDER BY RaisedDate DESC;
        """;

    using var reader =
        cmd.ExecuteReader();

    var count = 0;

    while (reader.Read())
    {
        count++;

        Console.WriteLine();
        Console.WriteLine(
            $"---------------- NCR #{count} ----------------");

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

        // Count linked repairs for this NCR.
        var ncrId =
            reader.IsDBNull(0)
                ? ""
                : reader.GetString(0);

        using var countConnection =
            new SqliteConnection(
                $"Data Source={db};Mode=ReadOnly");

        countConnection.Open();

        using var countCmd =
            countConnection.CreateCommand();

        countCmd.CommandText =
            """
            SELECT COUNT(*)
            FROM RepairRecords
            WHERE NcrId = $ncrId;
            """;

        countCmd.Parameters.AddWithValue(
            "$ncrId",
            ncrId);

        var repairCount =
            Convert.ToInt32(
                countCmd.ExecuteScalar());

        Console.WriteLine(
            $"  {"LinkedRepairs",-25}: {repairCount}");
    }

    if (count == 0)
    {
        Console.WriteLine();
        Console.WriteLine(
            "(No NCR records currently exist.)");
    }
}

// ==================================================
// TOTAL REPAIRS
// ==================================================

Console.WriteLine();
Console.WriteLine(
    "========== REPAIR SUMMARY ==========");

using (var cmd = connection.CreateCommand())
{
    cmd.CommandText =
        """
        SELECT COUNT(*)
        FROM RepairRecords;
        """;

    var total =
        Convert.ToInt32(
            cmd.ExecuteScalar());

    Console.WriteLine(
        $"Total RepairRecords: {total}");
}

// ==================================================
// EXISTING LINKS
// ==================================================

Console.WriteLine();
Console.WriteLine(
    "========== NCR -> REPAIR TRACEABILITY ==========");

using (var cmd = connection.CreateCommand())
{
    cmd.CommandText =
        """
        SELECT
            n.NcrNumber,
            n.Id AS NcrId,
            n.WeldId AS NcrWeldId,
            n.Status AS NcrStatus,
            n.DispositionType,
            r.Id AS RepairId,
            r.NcrId AS RepairNcrId,
            r.WeldId AS RepairWeldId,
            r.RepairNumber,
            r.Status AS RepairStatus,
            r.Reason
        FROM NcrRecords n
        LEFT JOIN RepairRecords r
            ON r.NcrId = n.Id
        ORDER BY n.RaisedDate DESC;
        """;

    using var reader =
        cmd.ExecuteReader();

    var count = 0;

    while (reader.Read())
    {
        count++;

        Console.WriteLine();
        Console.WriteLine(
            $"---------------- LINK #{count} ----------------");

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
                $"  {reader.GetName(i),-20}: {value}");
        }
    }

    if (count == 0)
    {
        Console.WriteLine(
            "(No NCR records available.)");
    }
}

Console.WriteLine();
Console.WriteLine(
    "==================================================");
Console.WriteLine(
    " CORRECTED PRECHECK COMPLETE");
Console.WriteLine(
    "==================================================");
