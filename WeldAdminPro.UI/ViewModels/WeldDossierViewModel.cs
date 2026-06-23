using System;
using System.Collections.ObjectModel;

namespace WeldAdminPro.UI.ViewModels
{
    public class WeldDossierViewModel
    {
        public Guid WeldId { get; set; }

        public string WeldNumber { get; set; }
            = "";

        public ObservableCollection<object> WeldInfo
        {
            get;
            set;
        }
            = new();

        public ObservableCollection<object> NdtHistory
        {
            get;
            set;
        }
            = new();

        public ObservableCollection<object> Ncrs
        {
            get;
            set;
        }
            = new();

        public ObservableCollection<object> HoldPoints
        {
            get;
            set;
        }
            = new();

        public string WpsNumber { get; set; }
            = "";

        public string WelderNumber { get; set; }
            = "";

        public string NdtStatus { get; set; }
            = "";

        public int RepairCount { get; set; }

        public string WorkflowStatus { get; set; }
            = "";
    }
}