using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class PqrDialogViewModel : ObservableObject
    {
        [ObservableProperty] private string pqrNumber = "";
        [ObservableProperty] private string process = "";
        [ObservableProperty] private string materialGroup = "";
        [ObservableProperty] private string pNumber = "";

        [ObservableProperty] private double thicknessQualifiedMin;
        [ObservableProperty] private double thicknessQualifiedMax;

        [ObservableProperty] private string fNumber = "";
        [ObservableProperty] private string qualifiedPosition = "";
        [ObservableProperty] private string jointType = "";

        [ObservableProperty] private double diameterMin;
        [ObservableProperty] private double diameterMax;

        public Pqr? Result { get; private set; }

        public Action? CloseAction { get; set; }

        [RelayCommand]
        private void Save()
        {
            Result = new Pqr
            {
                PqrNumber = PqrNumber,
                Process = Process,
                MaterialGroup = MaterialGroup,
                PNumber = PNumber,

                ThicknessQualifiedMin = ThicknessQualifiedMin,
                ThicknessQualifiedMax = ThicknessQualifiedMax,

                FNumber = FNumber,
                QualifiedPosition = QualifiedPosition,
                JointType = JointType,

                DiameterMin = DiameterMin,
                DiameterMax = DiameterMax
            };

            CloseAction?.Invoke();
        }
    }
}