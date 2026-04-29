using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Data.Repositories;

public partial class WpsDialogViewModel : ObservableObject
{
    private readonly WpsGeneratorService _generator = new();

    // =========================
    // CONSTRUCTOR
    // =========================
    public WpsDialogViewModel()
    {
        AvailablePqrs = new ObservableCollection<Pqr>(
            new PqrRepository().GetAll()
        );
    }

    // =========================
    // PROPERTIES
    // =========================

    public ObservableCollection<Pqr> AvailablePqrs { get; set; } = new();
    public ObservableCollection<string> ValidationMessages { get; } = new();

    [ObservableProperty] private string wpsNumber = "";
    [ObservableProperty] private string process = "";
    [ObservableProperty] private string materialGroup = "";
    [ObservableProperty] private string pNumber = "";

    [ObservableProperty] private double thicknessMin;
    [ObservableProperty] private double thicknessMax;
    [ObservableProperty] private double diameter;

    [ObservableProperty] private string position = "";
    [ObservableProperty] private string jointType = "";
    [ObservableProperty] private string fNumber = "";

    [ObservableProperty] private double preheatMin;
    [ObservableProperty] private double preheatMax;
    [ObservableProperty] private double? interpassMax;
    [ObservableProperty] private bool pwhtRequired;

    [ObservableProperty] private Pqr? selectedPqr;
    [ObservableProperty] private bool isLocked;

    [ObservableProperty] private string jointDesign = "";

    [ObservableProperty] private bool isCompliant;

    [ObservableProperty] private string validationStatus = "";
    public Wps? Result { get; private set; }

    public Action? CloseAction { get; set; }

    // =========================
    // AUTO GENERATE FROM PQR
    // =========================
    partial void OnSelectedPqrChanged(Pqr? value)
    {
        if (value == null)
            return;

        var generated = _generator.GenerateFromPqr(value);

        WpsNumber = generated.WpsNumber ?? "";
        Process = generated.Process ?? "";
        MaterialGroup = generated.MaterialGroup ?? "";

        PNumber = generated.PNumber ?? "";
        FNumber = generated.FNumber ?? "";

        ThicknessMin = generated.ThicknessMin;
        ThicknessMax = generated.ThicknessMax;
        Diameter = generated.Diameter;

        Position = generated.Position ?? "";
        JointType = generated.JointType ?? "";        
    }

    [RelayCommand]
    private void ApproveWps()
    {
        if (Result == null)
            return;

        Result.IsApproved = true;
        Result.IsLocked = true;
    }

    // =========================
    // SAVE
    // =========================
    [RelayCommand]
    private void Save()
    {
        Result = new Wps
        {
            WpsNumber = WpsNumber,
            Process = Process,
            MaterialGroup = MaterialGroup,
            PNumber = PNumber,

            ThicknessMin = ThicknessMin,
            ThicknessMax = ThicknessMax,
            Diameter = Diameter,

            Position = Position,
            JointType = JointType,
            FNumber = FNumber,

            PreheatMin = PreheatMin,
            PreheatMax = PreheatMax,
            InterpassMax = InterpassMax,
            PwhtRequired = PwhtRequired,

        };

        CloseAction?.Invoke();
    }
}