using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Core.Utilities;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Services;


namespace WeldAdminPro.UI.ViewModels.Quality
{
    public class NdtEntryViewModel : ObservableObject
    {
        private Guid _weldId;

        private string _weldNumber = string.Empty;

        private NdtMethodType _selectedMethod;

        private NdtResultType _selectedResult;

        private DateTime _inspectionDate = DateTime.Now;

        private string _inspectorName = string.Empty;

        private string _reportNumber = string.Empty;

        private string _acceptanceCriteria = string.Empty;

        private string _remarks = string.Empty;

        private bool _requiresRepair;

        private int _repairCycle;

        private bool _isReinspection;

        private Project? _currentProject;

        private readonly IHistoryTrackingService
            _historyService;


        public Guid WeldId
        {
            get => _weldId;
            set
            {
                _weldId = value;
                RaisePropertyChanged(nameof(WeldId));
            }
        }

        public string WeldNumber
        {
            get => _weldNumber;
            set
            {
                _weldNumber = value;
                RaisePropertyChanged(nameof(WeldNumber));
            }
        }

        public NdtMethodType SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                _selectedMethod = value;
                RaisePropertyChanged(nameof(SelectedMethod));
            }
        }

        public NdtResultType SelectedResult
        {
            get => _selectedResult;
            set
            {
                _selectedResult = value;

                RequiresRepair =
                    value == NdtResultType.Reject;

                RaisePropertyChanged(nameof(SelectedResult));
            }
        }

        public DateTime InspectionDate
        {
            get => _inspectionDate;
            set
            {
                _inspectionDate = value;
                RaisePropertyChanged(nameof(InspectionDate));
            }
        }

        public string InspectorName
        {
            get => _inspectorName;
            set
            {
                _inspectorName = value;
                RaisePropertyChanged(nameof(InspectorName));
            }
        }

        public string ReportNumber
        {
            get => _reportNumber;
            set
            {
                _reportNumber = value;
                RaisePropertyChanged(nameof(ReportNumber));
            }
        }

        public string AcceptanceCriteria
        {
            get => _acceptanceCriteria;
            set
            {
                _acceptanceCriteria = value;
                RaisePropertyChanged(nameof(AcceptanceCriteria));
            }
        }

        public string Remarks
        {
            get => _remarks;
            set
            {
                _remarks = value;
                RaisePropertyChanged(nameof(Remarks));
            }
        }

        public bool RequiresRepair
        {
            get => _requiresRepair;
            set
            {
                _requiresRepair = value;
                RaisePropertyChanged(nameof(RequiresRepair));
            }
        }

        public int RepairCycle
        {
            get => _repairCycle;
            set
            {
                _repairCycle = value;
                RaisePropertyChanged(nameof(RepairCycle));
            }
        }

        public bool IsReinspection
        {
            get => _isReinspection;
            set
            {
                _isReinspection = value;
                RaisePropertyChanged(nameof(IsReinspection));
            }
        }

        public ObservableCollection<NdtMethodType>
            AvailableMethods
        { get; }
            = new ObservableCollection<NdtMethodType>
            {
            NdtMethodType.VT,
            NdtMethodType.PT,
            NdtMethodType.MT,
            NdtMethodType.UT,
            NdtMethodType.RT
            };

        public ObservableCollection<NdtResultType>
            AvailableResults
        { get; }
            = new ObservableCollection<NdtResultType>
            {
            NdtResultType.Accept,
            NdtResultType.Reject,
            NdtResultType.ConditionalAccept
            };

        public ICommand? SaveCommand { get; }

        public NdtEntryViewModel()
        {
            _historyService =
                new HistoryTrackingService(
                    DatabasePath.GetConnectionString());

            App.ProjectContextService.ProjectChanged
                += OnProjectChanged;

            _currentProject =
                App.ProjectContextService.CurrentProject;
        }

        private void OnProjectChanged(Project? project)
        {
            _currentProject = project;
        }

        public WeldNdtResult BuildModel()
        {
            if (_currentProject == null)
            {
                throw new InvalidOperationException(
                    "No active project selected.");
            }

            _historyService.Track(new Weld 
            {
                Id = WeldId,
                WeldNumber = WeldNumber
            },
                    "NDT Inspection",
                    $"{SelectedMethod} inspection recorded. " +
                    $"Result: {SelectedResult}");

            return new WeldNdtResult
            {
                Id = Guid.NewGuid(),

                WeldId = WeldId,

                NdtMethod = SelectedMethod,

                Result = SelectedResult,

                InspectionDate = InspectionDate,

                InspectorName = InspectorName,

                ReportNumber = ReportNumber,

                AcceptanceCriteria = AcceptanceCriteria,

                Remarks = Remarks,

                RequiresRepair = RequiresRepair,

                RepairCycle = RepairCycle,

                IsReinspection = IsReinspection
            };
        }
    }

}
