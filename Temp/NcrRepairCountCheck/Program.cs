using Microsoft.Data.Sqlite;

var db = Path.Combine(
    Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData),
    "WeldAdminPro",
    "weldadmin.db");

using var connection =
    new SqliteConnection(
        $"Data Source={db};Mode=ReadOnly");

connection.Open();

using var cmd = connection.CreateCommand();

cmd.CommandText =
"""
SELECT
    n.Id AS NcrId,
    n.NcrNumber,
    n.WeldId,
    n.WeldNumber,
    n.Status,
    n.DispositionType,
    r.Id AS RepairId,
    r.RepairNumber,
    r.NcrId AS RepairNcrId,
    r.Status AS RepairStatus,
    r.Reason,
    r.RequestedDate,
    r.CompletedDate
FROM NcrRecords n
LEFT JOIN RepairRecords r
    ON r.NcrId = n.Id
WHERE n.NcrNumber = 'NCR-009'
ORDER BY r.RepairNumber;
""";
using var reader = cmd.ExecuteReader();

var count = 0;

while (reader.Read())
{
    count++;

    Console.WriteLine();
    Console.WriteLine($"Repair #{count}");

    for (var i = 0; i < reader.FieldCount; i++)
    {
        var value =
            reader.IsDBNull(i)
                ? "NULL"
                : Convert.ToString(reader.GetValue(i)) ?? "";

        Console.WriteLine(
            $"  {reader.GetName(i),-20}: {value}");
    }
}

Console.WriteLine();
Console.WriteLine(
    $"Repairs linked to NCR-009: {count}");

if (count == 1)
{
    Console.WriteLine(
        "PASS: NCR-009 still has exactly one repair.");
}
else
{
    Console.WriteLine(
        "FAIL: Expected exactly one repair.");
    Environment.ExitCode = 1;
}







