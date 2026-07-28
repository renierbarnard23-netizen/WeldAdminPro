using Microsoft.Data.Sqlite;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Data.Repositories
{
    public class PqrRepository
    {
        private string _connectionString => $"Data Source={DatabasePath.Get()}";

        // =========================
        // ADD
        // =========================
        public void Add(Pqr pqr)
        {
            if (pqr.Id == Guid.Empty)
            {
                pqr.Id = Guid.NewGuid();
            }

            if (string.IsNullOrWhiteSpace(
                    pqr.PqrNumber))
            {
                throw new InvalidOperationException(
                    "PQR Number is required.");
            }

            if (string.IsNullOrWhiteSpace(
                    pqr.Process))
            {
                throw new InvalidOperationException(
                    "Welding process is required.");
            }

            if (pqr.ThicknessTested < 0)
            {
                throw new InvalidOperationException(
                    "Thickness cannot be negative.");
            }

            pqr.PqrNumber = pqr.PqrNumber
                .Trim()
                .ToUpperInvariant();

            if (GetByNumber(
                    pqr.PqrNumber) != null)
            {
                throw new InvalidOperationException(
                    $"PQR '{pqr.PqrNumber}' already exists.");
            }

            using var connection =
                new SqliteConnection(
                    _connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
INSERT INTO Pqr (
    Id, PqrNumber, QualificationDate, QualifiedBy, ThicknessTested,
    Process, Standard, MaterialGroup, Position,
    FillerMaterial, GasType,
    AmpsUsed, VoltsUsed,
    HeatInput, Preheat, Interpass,
    PwhtPerformed, WpsId,

    PNumber, FNumber, QualifiedPosition, JointType, JointDesign,

    SurfacePreparation, GrooveAngle, RootFace, RootGap,
    Backing, BackGouging,

    ThicknessQualifiedMin, ThicknessQualifiedMax,
    DiameterMin, DiameterMax,
    WpsReferenceNumber,

    BaseMaterial1,
    BaseMaterial2,
    BaseMaterial1Specification,
    BaseMaterial2Specification,

    FillerClassification,
    ANumber,

    WeldingPosition,
    WeldingType,
    Progression,
    CurrentType,
    TransferMode,
    Polarity,

    TravelSpeed,

    ShieldingGas,
    GasFlowRate,

    BackingGas,
    BackingGasFlowRate,

    PreheatNotes,
    PreheatTemperature,
    PostWeldHeatTreatment,

    QualifiedPNumberRange,

    Revision,
    IsActive,
    IsPipe,

    PNumber2
    )
VALUES (
    $id, $number, $date, $by, $thickness,
    $process, $standard, $material, $position,
    $filler, $gas,
    $amps, $volts,
    $heat, $preheat, $interpass,
    $pwht, $wpsId,

    $pNumber, $fNumber, $qPosition, $joint, $jointDesign,

    $surfacePrep, $grooveAngle, $rootFace, $rootGap,
    $backing, $backGouging,

    $tMin, $tMax,
    $dMin, $dMax,
    $wpsRef,

    $base1, 
    $base2, 
    $base1Spec, 
    $base2Spec,

    $fillerClassification,
    $aNumber,

    $weldingPosition,
    $weldingType,
    $progression,
    $currentType,
    $transferMode,
    $polarity,

    $travelSpeed,

    $shieldingGas,
    $gasFlowRate,

$backingGas,
$backingGasFlowRate,

$preheatNotes,
$preheatTemperature,
$postWeldHeatTreatment,

$qualifiedPNumberRange,

$revision,
$isActive,
$isPipe,

$pNumber2
);";

            cmd.Parameters.AddWithValue("$id", pqr.Id.ToString());
            cmd.Parameters.AddWithValue("$number", pqr.PqrNumber ?? "");
            cmd.Parameters.AddWithValue("$date", pqr.QualificationDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$by", pqr.QualifiedBy ?? "");
            cmd.Parameters.AddWithValue("$thickness", pqr.ThicknessTested);

            cmd.Parameters.AddWithValue("$process", pqr.Process ?? "");
            cmd.Parameters.AddWithValue("$material", pqr.MaterialGroup ?? "");

            cmd.Parameters.AddWithValue("$base1", pqr.BaseMaterial1);
            cmd.Parameters.AddWithValue("$base2", pqr.BaseMaterial2);
            cmd.Parameters.AddWithValue("$base1Spec", pqr.BaseMaterial1Specification);
            cmd.Parameters.AddWithValue("$base2Spec", pqr.BaseMaterial2Specification);

            cmd.Parameters.AddWithValue("$position", pqr.Position ?? "");

            cmd.Parameters.AddWithValue("$filler", pqr.FillerMaterial ?? "");
            cmd.Parameters.AddWithValue("$gas", pqr.GasType ?? "");

            cmd.Parameters.AddWithValue("$amps", pqr.AmpsUsed);
            cmd.Parameters.AddWithValue("$volts", pqr.VoltsUsed);

            cmd.Parameters.AddWithValue("$heat", pqr.HeatInput);
            cmd.Parameters.AddWithValue("$preheat", pqr.Preheat);
            cmd.Parameters.AddWithValue("$interpass", pqr.Interpass);

            cmd.Parameters.AddWithValue("$pwht", pqr.PwhtPerformed ? 1 : 0);
            cmd.Parameters.AddWithValue("$wpsId", (object?)pqr.WpsId?.ToString() ?? DBNull.Value);

            // ESSENTIAL VARIABLES
            cmd.Parameters.AddWithValue("$pNumber", pqr.PNumber ?? "");
            cmd.Parameters.AddWithValue("$fNumber", pqr.FNumber ?? "");
            cmd.Parameters.AddWithValue("$qPosition", pqr.QualifiedPosition ?? "");
            cmd.Parameters.AddWithValue("$joint", pqr.JointType ?? "");

            cmd.Parameters.AddWithValue("$tMin", pqr.ThicknessQualifiedMin);
            cmd.Parameters.AddWithValue("$tMax", pqr.ThicknessQualifiedMax);

            cmd.Parameters.AddWithValue("$dMin", pqr.DiameterMin);
            cmd.Parameters.AddWithValue("$dMax", pqr.DiameterMax);

            cmd.Parameters.AddWithValue("$wpsRef", (object?)pqr.WpsReferenceNumber ?? DBNull.Value);

            cmd.Parameters.AddWithValue("$standard", pqr.Standard ?? "");
            cmd.Parameters.AddWithValue("$jointDesign", pqr.JointDesign ?? "");
            cmd.Parameters.AddWithValue("$surfacePrep", pqr.SurfacePreparation ?? "");
            cmd.Parameters.AddWithValue("$grooveAngle", pqr.GrooveAngle);
            cmd.Parameters.AddWithValue("$rootFace", pqr.RootFace);
            cmd.Parameters.AddWithValue("$rootGap", pqr.RootGap);
            cmd.Parameters.AddWithValue("$backing", pqr.Backing ?? "");
            cmd.Parameters.AddWithValue("$backGouging", pqr.BackGouging ?? "");

            cmd.Parameters.AddWithValue("$fillerClassification", pqr.FillerClassification ?? "");
            cmd.Parameters.AddWithValue("$aNumber", pqr.ANumber ?? "");

            cmd.Parameters.AddWithValue("$weldingPosition", pqr.WeldingPosition ?? "");
            cmd.Parameters.AddWithValue("$weldingType", pqr.WeldingType ?? "");
            cmd.Parameters.AddWithValue("$progression", pqr.Progression ?? "");
            cmd.Parameters.AddWithValue("$currentType", pqr.CurrentType ?? "");
            cmd.Parameters.AddWithValue("$transferMode", pqr.TransferMode ?? "");
            cmd.Parameters.AddWithValue("$polarity", pqr.Polarity ?? "");

            cmd.Parameters.AddWithValue("$travelSpeed", pqr.TravelSpeed);

            cmd.Parameters.AddWithValue("$shieldingGas", pqr.ShieldingGas ?? "");
            cmd.Parameters.AddWithValue("$gasFlowRate", pqr.GasFlowRate);

            cmd.Parameters.AddWithValue("$backingGas", pqr.BackingGas ?? "");
            cmd.Parameters.AddWithValue("$backingGasFlowRate", pqr.BackingGasFlowRate);

            cmd.Parameters.AddWithValue("$preheatNotes", pqr.PreheatNotes ?? "");
            cmd.Parameters.AddWithValue("$preheatTemperature", pqr.PreheatTemperature ?? "");
            cmd.Parameters.AddWithValue("$postWeldHeatTreatment", pqr.PostWeldHeatTreatment ?? "");

            cmd.Parameters.AddWithValue("$qualifiedPNumberRange", pqr.QualifiedPNumberRange ?? "");

            cmd.Parameters.AddWithValue("$revision", pqr.Revision);
            cmd.Parameters.AddWithValue("$isActive", pqr.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("$isPipe", pqr.IsPipe ? 1 : 0);

            cmd.Parameters.AddWithValue("$pNumber2", pqr.PNumber2 ?? "");

            cmd.ExecuteNonQuery();
        }

        // =========================
        // UPDATE
        // =========================

        public void Update(Pqr pqr)
        {
            if (pqr.Id == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "PQR Id cannot be empty.");
            }

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
UPDATE Pqr SET
    PqrNumber = $number,
    QualificationDate = $date,
    QualifiedBy = $by,
    ThicknessTested = $thickness,

    Process = $process,
    Standard = $standard,
    MaterialGroup = $material,
    Position = $position,

    FillerMaterial = $filler,
    GasType = $gas,

    AmpsUsed = $amps,
    VoltsUsed = $volts,
    HeatInput = $heat,

    Preheat = $preheat,
    Interpass = $interpass,
    PwhtPerformed = $pwht,
    WpsId = $wpsId,

    PNumber = $pNumber,
    FNumber = $fNumber,
    QualifiedPosition = $qPosition,
    JointType = $joint,
    JointDesign = $jointDesign,

    SurfacePreparation = $surfacePrep,
    GrooveAngle = $grooveAngle,
    RootFace = $rootFace,
    RootGap = $rootGap,
    Backing = $backing,
    BackGouging = $backGouging,

    ThicknessQualifiedMin = $tMin,
    ThicknessQualifiedMax = $tMax,

    DiameterMin = $dMin,
    DiameterMax = $dMax,

    WpsReferenceNumber = $wpsRef,

    IsApproved = $approved,
    ApprovedOn = $approvedOn,
    ApprovedBy = $approvedBy,
    IsLocked = $locked,

    JointDiagramPath = $jointDiagram,
    PassDiagramPath = $passDiagram,
    GrooveRadius = $grooveRadius,
    Misalignment = $misalignment,
    BackingType = $backingType,
    EdgePreparation = $edgePrep,

    BaseMaterial1=$BaseMaterial1,
    BaseMaterial2=$BaseMaterial2,
    BaseMaterial1Specification=$BaseMaterial1Specification,
    BaseMaterial2Specification=$BaseMaterial2Specification,

    FillerClassification = $fillerClassification,
ANumber = $aNumber,

WeldingPosition = $weldingPosition,
WeldingType = $weldingType,
Progression = $progression,
CurrentType = $currentType,
TransferMode = $transferMode,
Polarity = $polarity,

TravelSpeed = $travelSpeed,

ShieldingGas = $shieldingGas,
GasFlowRate = $gasFlowRate,

BackingGas = $backingGas,
BackingGasFlowRate = $backingGasFlowRate,

PreheatNotes = $preheatNotes,
PreheatTemperature = $preheatTemperature,
PostWeldHeatTreatment = $postWeldHeatTreatment,

QualifiedPNumberRange = $qualifiedPNumberRange,

Revision = $revision,
IsActive = $isActive,
IsPipe = $isPipe,

PNumber2 = $pNumber2

WHERE Id = $id;
";

            cmd.Parameters.AddWithValue("$id", pqr.Id.ToString());
            cmd.Parameters.AddWithValue("$number", pqr.PqrNumber ?? "");
            cmd.Parameters.AddWithValue("$date", pqr.QualificationDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$by", pqr.QualifiedBy ?? "");
            cmd.Parameters.AddWithValue("$thickness", pqr.ThicknessTested);

            cmd.Parameters.AddWithValue("$process", pqr.Process ?? "");
            cmd.Parameters.AddWithValue("$material", pqr.MaterialGroup ?? "");

            cmd.Parameters.AddWithValue("$BaseMaterial1", pqr.BaseMaterial1 ?? "");
            cmd.Parameters.AddWithValue("$BaseMaterial2", pqr.BaseMaterial2 ?? "");
            cmd.Parameters.AddWithValue("$BaseMaterial1Specification", pqr.BaseMaterial1Specification ?? "");
            cmd.Parameters.AddWithValue("$BaseMaterial2Specification", pqr.BaseMaterial2Specification ?? "");

            cmd.Parameters.AddWithValue("$position", pqr.Position ?? "");

            cmd.Parameters.AddWithValue("$filler", pqr.FillerMaterial ?? "");
            cmd.Parameters.AddWithValue("$gas", pqr.GasType ?? "");

            cmd.Parameters.AddWithValue("$amps", pqr.AmpsUsed);
            cmd.Parameters.AddWithValue("$volts", pqr.VoltsUsed);
            cmd.Parameters.AddWithValue("$heat", pqr.HeatInput);

            cmd.Parameters.AddWithValue("$preheat", pqr.Preheat);
            cmd.Parameters.AddWithValue("$interpass", pqr.Interpass);
            cmd.Parameters.AddWithValue("$pwht", pqr.PwhtPerformed ? 1 : 0);
            cmd.Parameters.AddWithValue("$wpsId", (object?)pqr.WpsId?.ToString() ?? DBNull.Value);

            cmd.Parameters.AddWithValue("$pNumber", (object?)pqr.PNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$fNumber", (object?)pqr.FNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$qPosition", (object?)pqr.QualifiedPosition ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$joint", (object?)pqr.JointType ?? DBNull.Value);

            cmd.Parameters.AddWithValue("$tMin", pqr.ThicknessQualifiedMin);
            cmd.Parameters.AddWithValue("$tMax", pqr.ThicknessQualifiedMax);

            cmd.Parameters.AddWithValue("$dMin", pqr.DiameterMin);
            cmd.Parameters.AddWithValue("$dMax", pqr.DiameterMax);

            cmd.Parameters.AddWithValue("$wpsRef", (object?)pqr.WpsReferenceNumber ?? DBNull.Value);

            cmd.Parameters.AddWithValue("$approved", pqr.IsApproved ? 1 : 0);
            cmd.Parameters.AddWithValue("$approvedOn", (object?)pqr.ApprovedOn?.ToString("s") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$approvedBy", (object?)pqr.ApprovedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$locked", pqr.IsLocked ? 1 : 0);

            cmd.Parameters.AddWithValue("$standard", pqr.Standard ?? "");
            cmd.Parameters.AddWithValue("$jointDesign", pqr.JointDesign ?? "");
            cmd.Parameters.AddWithValue("$surfacePrep", pqr.SurfacePreparation ?? "");
            cmd.Parameters.AddWithValue("$grooveAngle", pqr.GrooveAngle);
            cmd.Parameters.AddWithValue("$rootFace", pqr.RootFace);
            cmd.Parameters.AddWithValue("$rootGap", pqr.RootGap);
            cmd.Parameters.AddWithValue("$backing", pqr.Backing ?? "");
            cmd.Parameters.AddWithValue("$backGouging", pqr.BackGouging ?? "");

            cmd.Parameters.AddWithValue("$jointDiagram", pqr.JointDiagramPath ?? "");
            cmd.Parameters.AddWithValue("$passDiagram", pqr.PassDiagramPath ?? "");
            cmd.Parameters.AddWithValue("$grooveRadius", pqr.GrooveRadius);
            cmd.Parameters.AddWithValue("$misalignment", pqr.Misalignment);
            cmd.Parameters.AddWithValue("$backingType", pqr.BackingType ?? "");
            cmd.Parameters.AddWithValue("$edgePrep", pqr.EdgePreparation ?? "");

            cmd.Parameters.AddWithValue("$fillerClassification", pqr.FillerClassification ?? "");
            cmd.Parameters.AddWithValue("$aNumber", pqr.ANumber ?? "");

            cmd.Parameters.AddWithValue("$weldingPosition", pqr.WeldingPosition ?? "");
            cmd.Parameters.AddWithValue("$weldingType", pqr.WeldingType ?? "");
            cmd.Parameters.AddWithValue("$progression", pqr.Progression ?? "");
            cmd.Parameters.AddWithValue("$currentType", pqr.CurrentType ?? "");
            cmd.Parameters.AddWithValue("$transferMode", pqr.TransferMode ?? "");
            cmd.Parameters.AddWithValue("$polarity", pqr.Polarity ?? "");

            cmd.Parameters.AddWithValue("$travelSpeed", pqr.TravelSpeed);

            cmd.Parameters.AddWithValue("$shieldingGas", pqr.ShieldingGas ?? "");
            cmd.Parameters.AddWithValue("$gasFlowRate", pqr.GasFlowRate);

            cmd.Parameters.AddWithValue("$backingGas", pqr.BackingGas ?? "");
            cmd.Parameters.AddWithValue("$backingGasFlowRate", pqr.BackingGasFlowRate);

            cmd.Parameters.AddWithValue("$preheatNotes", pqr.PreheatNotes ?? "");
            cmd.Parameters.AddWithValue("$preheatTemperature", pqr.PreheatTemperature ?? "");
            cmd.Parameters.AddWithValue("$postWeldHeatTreatment", pqr.PostWeldHeatTreatment ?? "");

            cmd.Parameters.AddWithValue("$qualifiedPNumberRange", pqr.QualifiedPNumberRange ?? "");

            cmd.Parameters.AddWithValue("$revision", pqr.Revision);
            cmd.Parameters.AddWithValue("$isActive", pqr.IsActive ? 1 : 0);
            cmd.Parameters.AddWithValue("$isPipe", pqr.IsPipe ? 1 : 0);

            cmd.Parameters.AddWithValue("$pNumber2", pqr.PNumber2 ?? "");

            cmd.ExecuteNonQuery();
        }

        // =========================
        // GET BY NUMBER
        // =========================
        public Pqr? GetByNumber(string? pqrNumber)
        {
            if (string.IsNullOrWhiteSpace(pqrNumber))
                return null; // 🔥 prevent crash

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM Pqr WHERE PqrNumber = $num;";
            cmd.Parameters.AddWithValue("$num", pqrNumber);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return Map(reader);
        }

        public Pqr? GetById(Guid id)
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
SELECT *
FROM Pqr
WHERE Id = $id;";

            cmd.Parameters.AddWithValue(
                "$id",
                id.ToString());

            using var reader =
                cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return Map(reader);
        }

        // =========================
        // GET ALL
        // =========================
        public List<Pqr> GetAll()
        {
            var list = new List<Pqr>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT *
                FROM Pqr
                ORDER BY QualificationDate DESC;";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                list.Add(Map(reader));

            return list;
        }

        public PqrRepository()
        {
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            using var connection =
                new SqliteConnection(
                    _connectionString);

            connection.Open();

            using var cmd =
                connection.CreateCommand();

            cmd.CommandText = @"
CREATE INDEX IF NOT EXISTS
IX_Pqr_Number
ON Pqr(PqrNumber);

CREATE INDEX IF NOT EXISTS
IX_Pqr_WpsId
ON Pqr(WpsId);

CREATE INDEX IF NOT EXISTS
IX_Pqr_QualificationDate
ON Pqr(QualificationDate);

CREATE INDEX IF NOT EXISTS
IX_Pqr_Process
ON Pqr(Process);

CREATE INDEX IF NOT EXISTS
IX_Pqr_Approved
ON Pqr(IsApproved);
";

            cmd.ExecuteNonQuery();
        }

        // =========================
        // DELETE
        // =========================
        public void Delete(Guid id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Pqr WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", id.ToString());

            cmd.ExecuteNonQuery();
        }

        // =========================
        // MAPPER (SAFE)
        // =========================
        private Pqr Map(SqliteDataReader reader)
        {
            return new Pqr
            {
                Id = Guid.Parse(reader["Id"].ToString()!),
                PqrNumber = reader["PqrNumber"]?.ToString() ?? "",
                QualificationDate = DateTime.Parse(reader["QualificationDate"].ToString()!),
                QualifiedBy = reader["QualifiedBy"]?.ToString() ?? "",
                ThicknessTested = reader["ThicknessTested"] == DBNull.Value ? 0 : Convert.ToDouble(reader["ThicknessTested"]),

                Process = reader["Process"]?.ToString() ?? "",
                MaterialGroup = reader["MaterialGroup"]?.ToString() ?? "",

                BaseMaterial1 = reader["BaseMaterial1"]?.ToString() ?? "",
                BaseMaterial2 = reader["BaseMaterial2"]?.ToString() ?? "",

                BaseMaterial1Specification = reader["BaseMaterial1Specification"]?.ToString() ?? "",
                BaseMaterial2Specification = reader["BaseMaterial2Specification"]?.ToString() ?? "",

                Position = reader["Position"]?.ToString() ?? "",

                FillerMaterial = reader["FillerMaterial"]?.ToString() ?? "",
                GasType = reader["GasType"]?.ToString() ?? "",

                AmpsUsed = reader["AmpsUsed"] == DBNull.Value ? 0 : Convert.ToDouble(reader["AmpsUsed"]),
                VoltsUsed = reader["VoltsUsed"] == DBNull.Value ? 0 : Convert.ToDouble(reader["VoltsUsed"]),
                HeatInput = reader["HeatInput"] == DBNull.Value ? 0 : Convert.ToDouble(reader["HeatInput"]),

                Preheat = reader["Preheat"] == DBNull.Value ? 0 : Convert.ToDouble(reader["Preheat"]),
                Interpass = reader["Interpass"] == DBNull.Value ? 0 : Convert.ToDouble(reader["Interpass"]),
                PwhtPerformed = reader["PwhtPerformed"] != DBNull.Value && Convert.ToInt32(reader["PwhtPerformed"]) == 1,

                WpsId = reader["WpsId"] == DBNull.Value ? null : Guid.Parse(reader["WpsId"].ToString()!),

                // ESSENTIAL
                PNumber = reader["PNumber"]?.ToString() ?? "",
                FNumber = reader["FNumber"]?.ToString() ?? "",
                QualifiedPosition = reader["QualifiedPosition"]?.ToString() ?? "",
                JointType = reader["JointType"]?.ToString() ?? "",

                ThicknessQualifiedMin = reader["ThicknessQualifiedMin"] == DBNull.Value ? 0 : Convert.ToDouble(reader["ThicknessQualifiedMin"]),
                ThicknessQualifiedMax = reader["ThicknessQualifiedMax"] == DBNull.Value ? 0 : Convert.ToDouble(reader["ThicknessQualifiedMax"]),

                DiameterMin = reader["DiameterMin"] == DBNull.Value ? 0 : Convert.ToDouble(reader["DiameterMin"]),
                DiameterMax = reader["DiameterMax"] == DBNull.Value ? 0 : Convert.ToDouble(reader["DiameterMax"]),

                WpsReferenceNumber = reader["WpsReferenceNumber"]?.ToString() ?? "",

                IsApproved = reader["IsApproved"] != DBNull.Value && Convert.ToInt32(reader["IsApproved"]) == 1,
                ApprovedOn = reader["ApprovedOn"] == DBNull.Value ? null : DateTime.Parse(reader["ApprovedOn"].ToString()!),
                ApprovedBy = reader["ApprovedBy"]?.ToString(),
                IsLocked = reader["IsLocked"] != DBNull.Value && Convert.ToInt32(reader["IsLocked"]) == 1,

                Standard = reader["Standard"]?.ToString() ?? "",
                JointDesign = reader["JointDesign"]?.ToString() ?? "",
                SurfacePreparation = reader["SurfacePreparation"]?.ToString() ?? "",

                JointDiagramPath = reader["JointDiagramPath"]?.ToString() ?? "",

                PassDiagramPath = reader["PassDiagramPath"]?.ToString() ?? "",

                GrooveRadius = reader["GrooveRadius"] == 
                    DBNull.Value
                    ? 0
                    : Convert.ToDouble(reader["GrooveRadius"]),

                Misalignment = reader["Misalignment"] ==
                    DBNull.Value
                    ? 0
                    : Convert.ToDouble(reader["Misalignment"]),

                BackingType = reader["BackingType"]?.ToString() ?? "",

                EdgePreparation = reader["EdgePreparation"]?.ToString() ?? "",

                GrooveAngle = reader["GrooveAngle"] == DBNull.Value ? 0 : Convert.ToDouble(reader["GrooveAngle"]),
                RootFace = reader["RootFace"] == DBNull.Value ? 0 : Convert.ToDouble(reader["RootFace"]),
                RootGap = reader["RootGap"] == DBNull.Value ? 0 : Convert.ToDouble(reader["RootGap"]),

                Backing = reader["Backing"]?.ToString() ?? "",
                BackGouging = reader["BackGouging"]?.ToString() ?? "",

                FillerClassification = reader["FillerClassification"]?.ToString() ?? "",
                ANumber = reader["ANumber"]?.ToString() ?? "",

                WeldingPosition = reader["WeldingPosition"]?.ToString() ?? "",
                WeldingType = reader["WeldingType"]?.ToString() ?? "",
                Progression = reader["Progression"]?.ToString() ?? "",
                CurrentType = reader["CurrentType"]?.ToString() ?? "",
                TransferMode = reader["TransferMode"]?.ToString() ?? "",
                Polarity = reader["Polarity"]?.ToString() ?? "",

                TravelSpeed = reader["TravelSpeed"] == DBNull.Value ? 0 : Convert.ToDouble(reader["TravelSpeed"]),

                ShieldingGas = reader["ShieldingGas"]?.ToString() ?? "",
                GasFlowRate = reader["GasFlowRate"] == DBNull.Value ? 0 : Convert.ToDouble(reader["GasFlowRate"]),

                BackingGas = reader["BackingGas"]?.ToString() ?? "",
                BackingGasFlowRate = reader["BackingGasFlowRate"] == DBNull.Value ? 0 : Convert.ToDouble(reader["BackingGasFlowRate"]),

                PreheatNotes = reader["PreheatNotes"]?.ToString() ?? "",
                PreheatTemperature = reader["PreheatTemperature"]?.ToString() ?? "",
                PostWeldHeatTreatment = reader["PostWeldHeatTreatment"]?.ToString() ?? "",

                QualifiedPNumberRange = reader["QualifiedPNumberRange"]?.ToString() ?? "",

                Revision = reader["Revision"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Revision"]),
                IsActive = reader["IsActive"] != DBNull.Value && Convert.ToInt32(reader["IsActive"]) == 1,
                IsPipe = reader["IsPipe"] != DBNull.Value && Convert.ToInt32(reader["IsPipe"]) == 1,

                PNumber2 = reader["PNumber2"]?.ToString() ?? "",
            };
            
    }
    }
}