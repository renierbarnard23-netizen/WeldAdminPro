using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Web.Services.Quality;

public class MaterialLibraryService
{
    public IReadOnlyList<BaseMaterial> BaseMaterials { get; } = new List<BaseMaterial>
{
    // =====================================================
    // CARBON STEELS (ASME IX P-No.1)
    // =====================================================

    new()
    {
        Material = "Carbon Steel Plate",
        Specification = "ASME SA-36",
        Grade = "",
        UNS = "K02600",
        Category = "Carbon Steel",
        Description = "Structural Carbon Steel Plate",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Pipe",
        Specification = "ASME SA-53",
        Grade = "Grade A",
        UNS = "K03006",
        Category = "Carbon Steel",
        Description = "Seamless / Welded Pipe",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Pipe",
        Specification = "ASME SA-53",
        Grade = "Grade B",
        UNS = "K03006",
        Category = "Carbon Steel",
        Description = "Seamless / Welded Pipe",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Forging",
        Specification = "ASME SA-105",
        Grade = "",
        UNS = "K03504",
        Category = "Carbon Steel",
        Description = "Forged Carbon Steel",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Pipe",
        Specification = "ASME SA-106",
        Grade = "Grade A",
        UNS = "K03006",
        Category = "Carbon Steel",
        Description = "Seamless Pipe",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Pipe",
        Specification = "ASME SA-106",
        Grade = "Grade B",
        UNS = "K03006",
        Category = "Carbon Steel",
        Description = "Seamless Pipe",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Pipe",
        Specification = "ASME SA-106",
        Grade = "Grade C",
        UNS = "K03006",
        Category = "Carbon Steel",
        Description = "Seamless Pipe",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Tube",
        Specification = "ASME SA-179",
        Grade = "",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Heat Exchanger Tube",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Boiler Tube",
        Specification = "ASME SA-192",
        Grade = "",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Boiler Tube",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Boiler Tube",
        Specification = "ASME SA-210",
        Grade = "Grade A1",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Medium Carbon Steel Boiler Tube",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Boiler Tube",
        Specification = "ASME SA-210",
        Grade = "Grade C",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Medium Carbon Steel Boiler Tube",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Fitting",
        Specification = "ASME SA-234",
        Grade = "WPB",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Wrought Butt Welding Fittings",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Plate",
        Specification = "ASME SA-283",
        Grade = "Grade C",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Pressure Vessel Plate",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Plate",
        Specification = "ASME SA-285",
        Grade = "Grade C",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Pressure Vessel Plate",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Plate",
        Specification = "ASME SA-299",
        Grade = "",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Pressure Vessel Plate",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Plate",
        Specification = "ASME SA-515",
        Grade = "Grade 60",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Intermediate Temperature Pressure Vessel Plate",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Plate",
        Specification = "ASME SA-515",
        Grade = "Grade 70",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Intermediate Temperature Pressure Vessel Plate",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Plate",
        Specification = "ASME SA-516",
        Grade = "Grade 55",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Pressure Vessel Plate",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Plate",
        Specification = "ASME SA-516",
        Grade = "Grade 60",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Pressure Vessel Plate",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Plate",
        Specification = "ASME SA-516",
        Grade = "Grade 65",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Pressure Vessel Plate",
        PNumber = 1,
        GroupNumber = 1
    },

    new()
    {
        Material = "Carbon Steel Plate",
        Specification = "ASME SA-516",
        Grade = "Grade 70",
        UNS = "",
        Category = "Carbon Steel",
        Description = "Pressure Vessel Plate",
        PNumber = 1,
        GroupNumber = 1
    },

// =====================================================
// LOW ALLOY STEELS
// =====================================================

new()
{
    Material = "Chrome-Moly Pipe",
    Specification = "ASME SA-335",
    Grade = "P1",
    UNS = "K11597",
    Category = "Low Alloy Steel",
    Description = "0.5Mo Seamless Pipe",
    PNumber = 3,
    GroupNumber = 1
},

new()
{
    Material = "Chrome-Moly Tube",
    Specification = "ASME SA-213",
    Grade = "T2",
    UNS = "K11597",
    Category = "Low Alloy Steel",
    Description = "0.5Mo Boiler Tube",
    PNumber = 3,
    GroupNumber = 1
},

new()
{
    Material = "Chrome-Moly Plate",
    Specification = "ASME SA-204",
    Grade = "Grade A",
    UNS = "",
    Category = "Low Alloy Steel",
    Description = "0.5Mo Pressure Vessel Plate",
    PNumber = 3,
    GroupNumber = 1
},

new()
{
    Material = "Chrome-Moly Plate",
    Specification = "ASME SA-387",
    Grade = "Grade 11",
    UNS = "",
    Category = "Low Alloy Steel",
    Description = "1.25Cr-0.5Mo Pressure Vessel Plate",
    PNumber = 4,
    GroupNumber = 1
},

new()
{
    Material = "Chrome-Moly Pipe",
    Specification = "ASME SA-335",
    Grade = "P11",
    UNS = "K11597",
    Category = "Low Alloy Steel",
    Description = "1.25Cr-0.5Mo Seamless Pipe",
    PNumber = 4,
    GroupNumber = 1
},

new()
{
    Material = "Chrome-Moly Tube",
    Specification = "ASME SA-213",
    Grade = "T11",
    UNS = "K11597",
    Category = "Low Alloy Steel",
    Description = "1.25Cr-0.5Mo Boiler Tube",
    PNumber = 4,
    GroupNumber = 1
},

new()
{
    Material = "Chrome-Moly Plate",
    Specification = "ASME SA-387",
    Grade = "Grade 22",
    UNS = "",
    Category = "Low Alloy Steel",
    Description = "2.25Cr-1Mo Pressure Vessel Plate",
    PNumber = 5,
    GroupNumber = 1
},

new()
{
    Material = "Chrome-Moly Pipe",
    Specification = "ASME SA-335",
    Grade = "P22",
    UNS = "K21590",
    Category = "Low Alloy Steel",
    Description = "2.25Cr-1Mo Seamless Pipe",
    PNumber = 5,
    GroupNumber = 1
},

new()
{
    Material = "Chrome-Moly Tube",
    Specification = "ASME SA-213",
    Grade = "T22",
    UNS = "K21590",
    Category = "Low Alloy Steel",
    Description = "2.25Cr-1Mo Boiler Tube",
    PNumber = 5,
    GroupNumber = 1
},

new()
{
    Material = "Chrome-Moly Pipe",
    Specification = "ASME SA-335",
    Grade = "P5",
    UNS = "K41545",
    Category = "Low Alloy Steel",
    Description = "5Cr-0.5Mo Seamless Pipe",
    PNumber = 5,
    GroupNumber = 1
},

new()
{
    Material = "Chrome-Moly Pipe",
    Specification = "ASME SA-335",
    Grade = "P9",
    UNS = "S50400",
    Category = "Low Alloy Steel",
    Description = "9Cr-1Mo Seamless Pipe",
    PNumber = 5,
    GroupNumber = 1
},

new()
{
    Material = "Chrome-Moly Pipe",
    Specification = "ASME SA-335",
    Grade = "P91",
    UNS = "K91560",
    Category = "Low Alloy Steel",
    Description = "9Cr-1Mo-V High Temperature Pipe",
    PNumber = 5,
    GroupNumber = 2
},

    // =====================================================
// STAINLESS STEELS (ASME IX P-No.8)
// =====================================================

new()
{
    Material = "Austenitic Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "304",
    UNS = "S30400",
    Category = "Stainless Steel",
    Description = "18Cr-8Ni Stainless Plate",
    PNumber = 8,
    GroupNumber = 1
},

new()
{
    Material = "Austenitic Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "304L",
    UNS = "S30403",
    Category = "Stainless Steel",
    Description = "Low Carbon Stainless Plate",
    PNumber = 8,
    GroupNumber = 1
},

new()
{
    Material = "Austenitic Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "304H",
    UNS = "S30409",
    Category = "Stainless Steel",
    Description = "High Temperature Stainless Plate",
    PNumber = 8,
    GroupNumber = 1
},

new()
{
    Material = "Austenitic Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "309",
    UNS = "S30900",
    Category = "Stainless Steel",
    Description = "Heat Resistant Stainless Plate",
    PNumber = 8,
    GroupNumber = 1
},

new()
{
    Material = "Austenitic Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "310",
    UNS = "S31000",
    Category = "Stainless Steel",
    Description = "High Temperature Stainless Plate",
    PNumber = 8,
    GroupNumber = 1
},

new()
{
    Material = "Austenitic Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "316",
    UNS = "S31600",
    Category = "Stainless Steel",
    Description = "Molybdenum Bearing Stainless Plate",
    PNumber = 8,
    GroupNumber = 1
},

new()
{
    Material = "Austenitic Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "316L",
    UNS = "S31603",
    Category = "Stainless Steel",
    Description = "Low Carbon Moly Stainless Plate",
    PNumber = 8,
    GroupNumber = 1
},

new()
{
    Material = "Austenitic Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "316H",
    UNS = "S31609",
    Category = "Stainless Steel",
    Description = "High Temperature Moly Stainless Plate",
    PNumber = 8,
    GroupNumber = 1
},

new()
{
    Material = "Austenitic Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "317L",
    UNS = "S31703",
    Category = "Stainless Steel",
    Description = "High Corrosion Resistant Stainless Plate",
    PNumber = 8,
    GroupNumber = 1
},

new()
{
    Material = "Austenitic Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "321",
    UNS = "S32100",
    Category = "Stainless Steel",
    Description = "Titanium Stabilized Stainless Plate",
    PNumber = 8,
    GroupNumber = 1
},

new()
{
    Material = "Austenitic Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "347",
    UNS = "S34700",
    Category = "Stainless Steel",
    Description = "Niobium Stabilized Stainless Plate",
    PNumber = 8,
    GroupNumber = 1
},

new()
{
    Material = "Austenitic Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "904L",
    UNS = "N08904",
    Category = "Stainless Steel",
    Description = "High Alloy Austenitic Stainless Plate",
    PNumber = 8,
    GroupNumber = 1
},

    // =====================================================
// DUPLEX & SUPER DUPLEX STAINLESS STEELS
// =====================================================

new()
{
    Material = "Duplex Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "2205",
    UNS = "S31803",
    Category = "Duplex Stainless Steel",
    Description = "22Cr Duplex Stainless Plate",
    PNumber = 10,
    GroupNumber = 1
},

new()
{
    Material = "Duplex Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "2205",
    UNS = "S32205",
    Category = "Duplex Stainless Steel",
    Description = "22Cr Duplex Stainless Plate",
    PNumber = 10,
    GroupNumber = 1
},

new()
{
    Material = "Super Duplex Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "2507",
    UNS = "S32750",
    Category = "Super Duplex Stainless Steel",
    Description = "25Cr Super Duplex Stainless Plate",
    PNumber = 10,
    GroupNumber = 2
},

new()
{
    Material = "Super Duplex Stainless Plate",
    Specification = "ASME SA-240",
    Grade = "2507",
    UNS = "S32760",
    Category = "Super Duplex Stainless Steel",
    Description = "25Cr Super Duplex Stainless Plate",
    PNumber = 10,
    GroupNumber = 2
},

new()
{
    Material = "Duplex Stainless Pipe",
    Specification = "ASME SA-790",
    Grade = "UNS S31803",
    UNS = "S31803",
    Category = "Duplex Stainless Steel",
    Description = "Seamless and Welded Duplex Pipe",
    PNumber = 10,
    GroupNumber = 1
},

new()
{
    Material = "Duplex Stainless Pipe",
    Specification = "ASME SA-790",
    Grade = "UNS S32205",
    UNS = "S32205",
    Category = "Duplex Stainless Steel",
    Description = "Seamless and Welded Duplex Pipe",
    PNumber = 10,
    GroupNumber = 1
},

new()
{
    Material = "Super Duplex Stainless Pipe",
    Specification = "ASME SA-790",
    Grade = "UNS S32750",
    UNS = "S32750",
    Category = "Super Duplex Stainless Steel",
    Description = "Seamless and Welded Super Duplex Pipe",
    PNumber = 10,
    GroupNumber = 2
},

new()
{
    Material = "Super Duplex Stainless Pipe",
    Specification = "ASME SA-790",
    Grade = "UNS S32760",
    UNS = "S32760",
    Category = "Super Duplex Stainless Steel",
    Description = "Seamless and Welded Super Duplex Pipe",
    PNumber = 10,
    GroupNumber = 2
},

new()
{
    Material = "Duplex Stainless Forging",
    Specification = "ASME SA-182",
    Grade = "F51",
    UNS = "S31803",
    Category = "Duplex Stainless Steel",
    Description = "Duplex Stainless Forging",
    PNumber = 10,
    GroupNumber = 1
},

new()
{
    Material = "Super Duplex Stainless Forging",
    Specification = "ASME SA-182",
    Grade = "F53",
    UNS = "S32750",
    Category = "Super Duplex Stainless Steel",
    Description = "Super Duplex Stainless Forging",
    PNumber = 10,
    GroupNumber = 2
},

new()
{
    Material = "Super Duplex Stainless Forging",
    Specification = "ASME SA-182",
    Grade = "F55",
    UNS = "S32760",
    Category = "Super Duplex Stainless Steel",
    Description = "Super Duplex Stainless Forging",
    PNumber = 10,
    GroupNumber = 2
},

    // =====================================================
// NICKEL & HIGH NICKEL ALLOYS (ASME IX P-No. 41–45)
// =====================================================

new()
{
    Material = "Nickel Alloy Plate",
    Specification = "ASME SB-168",
    Grade = "Alloy 600",
    UNS = "N06600",
    Category = "Nickel Alloy",
    Description = "Inconel 600 Plate",
    PNumber = 43,
    GroupNumber = 1
},

new()
{
    Material = "Nickel Alloy Plate",
    Specification = "ASME SB-168",
    Grade = "Alloy 601",
    UNS = "N06601",
    Category = "Nickel Alloy",
    Description = "Inconel 601 Plate",
    PNumber = 43,
    GroupNumber = 1
},

new()
{
    Material = "Nickel Alloy Plate",
    Specification = "ASME SB-443",
    Grade = "Alloy 625",
    UNS = "N06625",
    Category = "Nickel Alloy",
    Description = "Inconel 625 Plate",
    PNumber = 43,
    GroupNumber = 1
},

new()
{
    Material = "Nickel Alloy Forging",
    Specification = "ASME SB-564",
    Grade = "Alloy 625",
    UNS = "N06625",
    Category = "Nickel Alloy",
    Description = "Inconel 625 Forging",
    PNumber = 43,
    GroupNumber = 1
},

new()
{
    Material = "Nickel Alloy Plate",
    Specification = "ASME SB-424",
    Grade = "Alloy 825",
    UNS = "N08825",
    Category = "Nickel Alloy",
    Description = "Incoloy 825 Plate",
    PNumber = 45,
    GroupNumber = 1
},

new()
{
    Material = "Nickel Alloy Plate",
    Specification = "ASME SB-409",
    Grade = "Alloy 800H",
    UNS = "N08810",
    Category = "Nickel Alloy",
    Description = "Incoloy 800H Plate",
    PNumber = 45,
    GroupNumber = 1
},

new()
{
    Material = "Nickel Alloy Plate",
    Specification = "ASME SB-409",
    Grade = "Alloy 800HT",
    UNS = "N08811",
    Category = "Nickel Alloy",
    Description = "Incoloy 800HT Plate",
    PNumber = 45,
    GroupNumber = 1
},

new()
{
    Material = "Nickel-Copper Alloy",
    Specification = "ASME SB-127",
    Grade = "Alloy 400",
    UNS = "N04400",
    Category = "Nickel-Copper",
    Description = "Monel 400 Plate",
    PNumber = 42,
    GroupNumber = 1
},

new()
{
    Material = "Nickel-Copper Alloy",
    Specification = "ASME SB-127",
    Grade = "K-500",
    UNS = "N05500",
    Category = "Nickel-Copper",
    Description = "Monel K-500 Plate",
    PNumber = 42,
    GroupNumber = 1
},

new()
{
    Material = "Nickel-Molybdenum-Chromium Alloy",
    Specification = "ASME SB-575",
    Grade = "C-276",
    UNS = "N10276",
    Category = "Hastelloy",
    Description = "Hastelloy C-276 Plate",
    PNumber = 44,
    GroupNumber = 1
},

new()
{
    Material = "Nickel-Molybdenum-Chromium Alloy",
    Specification = "ASME SB-575",
    Grade = "C-22",
    UNS = "N06022",
    Category = "Hastelloy",
    Description = "Hastelloy C-22 Plate",
    PNumber = 44,
    GroupNumber = 1
},

    // =====================================================
// ALUMINIUM ALLOYS (ASME IX P-No.21)
// =====================================================

new()
{
    Material = "Aluminium Plate",
    Specification = "ASTM B209",
    Grade = "5052-H32",
    UNS = "A95052",
    Category = "Aluminium",
    Description = "Corrosion Resistant Aluminium Plate",
    PNumber = 21,
    GroupNumber = 1
},

new()
{
    Material = "Aluminium Plate",
    Specification = "ASTM B209",
    Grade = "5083-H321",
    UNS = "A95083",
    Category = "Aluminium",
    Description = "Marine Grade Aluminium Plate",
    PNumber = 21,
    GroupNumber = 1
},

new()
{
    Material = "Aluminium Plate",
    Specification = "ASTM B209",
    Grade = "5086-H116",
    UNS = "A95086",
    Category = "Aluminium",
    Description = "Marine Aluminium Plate",
    PNumber = 21,
    GroupNumber = 1
},

new()
{
    Material = "Aluminium Plate",
    Specification = "ASTM B209",
    Grade = "5454-H32",
    UNS = "A95454",
    Category = "Aluminium",
    Description = "Pressure Vessel Aluminium Plate",
    PNumber = 21,
    GroupNumber = 1
},

new()
{
    Material = "Aluminium Plate",
    Specification = "ASTM B209",
    Grade = "6061-T6",
    UNS = "A96061",
    Category = "Aluminium",
    Description = "Structural Aluminium Plate",
    PNumber = 21,
    GroupNumber = 2
},

new()
{
    Material = "Aluminium Plate",
    Specification = "ASTM B209",
    Grade = "6063-T6",
    UNS = "A96063",
    Category = "Aluminium",
    Description = "Architectural Aluminium",
    PNumber = 21,
    GroupNumber = 2
},

new()
{
    Material = "Aluminium Plate",
    Specification = "ASTM B209",
    Grade = "6082-T6",
    UNS = "A96082",
    Category = "Aluminium",
    Description = "High Strength Structural Aluminium",
    PNumber = 21,
    GroupNumber = 2
},

    // =====================================================
// TITANIUM ALLOYS (ASME IX P-No.51)
// =====================================================

new()
{
    Material = "Titanium Plate",
    Specification = "ASTM B265",
    Grade = "Grade 2",
    UNS = "R50400",
    Category = "Titanium",
    Description = "Commercially Pure Titanium",
    PNumber = 51,
    GroupNumber = 1
},

new()
{
    Material = "Titanium Plate",
    Specification = "ASTM B265",
    Grade = "Grade 5",
    UNS = "R56400",
    Category = "Titanium",
    Description = "Ti-6Al-4V Titanium Alloy",
    PNumber = 51,
    GroupNumber = 2
},

new()
{
    Material = "Titanium Plate",
    Specification = "ASTM B265",
    Grade = "Grade 7",
    UNS = "R52400",
    Category = "Titanium",
    Description = "Palladium Alloyed Titanium",
    PNumber = 51,
    GroupNumber = 1
},

    

    };
    public IReadOnlyList<FillerMaterial> Fillers { get; } = new List<FillerMaterial>
{
    // =====================================================
    // SMAW ELECTRODES
    // =====================================================

    new()
    {
        Classification = "E6010",
        AwsClassification = "AWS A5.1",
        SfaNumber = "SFA-5.1",
        FillerForm = "Covered Electrode",
        FillerComposition = "Carbon Steel",
        FNumber = 3,
        ANumber = 1
    },

    new()
    {
        Classification = "E6011",
        AwsClassification = "AWS A5.1",
        SfaNumber = "SFA-5.1",
        FillerForm = "Covered Electrode",
        FillerComposition = "Carbon Steel",
        FNumber = 3,
        ANumber = 1
    },

    new()
    {
        Classification = "E6013",
        AwsClassification = "AWS A5.1",
        SfaNumber = "SFA-5.1",
        FillerForm = "Covered Electrode",
        FillerComposition = "Carbon Steel",
        FNumber = 2,
        ANumber = 1
    },

    new()
    {
        Classification = "E7018",
        AwsClassification = "AWS A5.1",
        SfaNumber = "SFA-5.1",
        FillerForm = "Covered Electrode",
        FillerComposition = "Low Hydrogen Carbon Steel",
        FNumber = 4,
        ANumber = 1
    },

    new()
    {
        Classification = "E8018-B2",
        AwsClassification = "AWS A5.5",
        SfaNumber = "SFA-5.5",
        FillerForm = "Covered Electrode",
        FillerComposition = "1.25Cr-0.5Mo",
        FNumber = 4,
        ANumber = 2
    },

    new()
    {
        Classification = "E9018-B3",
        AwsClassification = "AWS A5.5",
        SfaNumber = "SFA-5.5",
        FillerForm = "Covered Electrode",
        FillerComposition = "2.25Cr-1Mo",
        FNumber = 4,
        ANumber = 3
    },

    // =====================================================
    // GTAW / GMAW
    // =====================================================

    new()
    {
        Classification = "ER70S-2",
        AwsClassification = "AWS A5.18",
        SfaNumber = "SFA-5.18",
        FillerForm = "Solid Wire",
        FillerComposition = "Carbon Steel",
        FNumber = 6,
        ANumber = 1
    },

    new()
    {
        Classification = "ER70S-6",
        AwsClassification = "AWS A5.18",
        SfaNumber = "SFA-5.18",
        FillerForm = "Solid Wire",
        FillerComposition = "Carbon Steel",
        FNumber = 6,
        ANumber = 1
    },

    new()
    {
        Classification = "ER308L",
        AwsClassification = "AWS A5.9",
        SfaNumber = "SFA-5.9",
        FillerForm = "Solid Wire",
        FillerComposition = "304L Stainless",
        FNumber = 6,
        ANumber = 8
    },

    new()
    {
        Classification = "ER309L",
        AwsClassification = "AWS A5.9",
        SfaNumber = "SFA-5.9",
        FillerForm = "Solid Wire",
        FillerComposition = "309L Stainless",
        FNumber = 6,
        ANumber = 8
    },

    new()
    {
        Classification = "ER316L",
        AwsClassification = "AWS A5.9",
        SfaNumber = "SFA-5.9",
        FillerForm = "Solid Wire",
        FillerComposition = "316L Stainless",
        FNumber = 6,
        ANumber = 8
    },

    new()
    {
        Classification = "ER347",
        AwsClassification = "AWS A5.9",
        SfaNumber = "SFA-5.9",
        FillerForm = "Solid Wire",
        FillerComposition = "347 Stainless",
        FNumber = 6,
        ANumber = 8
    },

    // =====================================================
    // DUPLEX
    // =====================================================

    new()
    {
        Classification = "ER2209",
        AwsClassification = "AWS A5.9",
        SfaNumber = "SFA-5.9",
        FillerForm = "Solid Wire",
        FillerComposition = "Duplex Stainless",
        FNumber = 6,
        ANumber = 9
    },

    // =====================================================
    // NICKEL
    // =====================================================

    new()
    {
        Classification = "ERNiCr-3",
        AwsClassification = "AWS A5.14",
        SfaNumber = "SFA-5.14",
        FillerForm = "Solid Wire",
        FillerComposition = "Inconel 600",
        FNumber = 43,
        ANumber = 0
    },

    new()
    {
        Classification = "ERNiCrMo-3",
        AwsClassification = "AWS A5.14",
        SfaNumber = "SFA-5.14",
        FillerForm = "Solid Wire",
        FillerComposition = "Inconel 625",
        FNumber = 43,
        ANumber = 0
    },

    new()
    {
        Classification = "ERNiMo-3",
        AwsClassification = "AWS A5.14",
        SfaNumber = "SFA-5.14",
        FillerForm = "Solid Wire",
        FillerComposition = "Nickel Alloy",
        FNumber = 45,
        ANumber = 0
    },

    // =====================================================
    // ALUMINIUM
    // =====================================================

    new()
    {
        Classification = "ER4043",
        AwsClassification = "AWS A5.10",
        SfaNumber = "SFA-5.10",
        FillerForm = "Solid Wire",
        FillerComposition = "Aluminium",
        FNumber = 23,
        ANumber = 0
    },

    new()
    {
        Classification = "ER5356",
        AwsClassification = "AWS A5.10",
        SfaNumber = "SFA-5.10",
        FillerForm = "Solid Wire",
        FillerComposition = "Aluminium",
        FNumber = 23,
        ANumber = 0
    }
};
}