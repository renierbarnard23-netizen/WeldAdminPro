using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Quality;

public partial class WpsDialogViewModel : ObservableObject
{
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

    public Wps? Result { get; private set; }

    public Action? CloseAction { get; set; }

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
            PwhtRequired = PwhtRequired
        };

        CloseAction?.Invoke();
    }
}