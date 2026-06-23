using System;
using System.Windows;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.UI.ViewModels;
using WeldAdminPro.UI.Views.Quality;

namespace WeldAdminPro.UI.Views.Quality
{
    public partial class WeldDossierWindow
        : Window
    {
        public WeldDossierWindow(
            WeldDossierViewModel viewModel)
        {
            InitializeComponent();

            DataContext =
                viewModel;

            AttachmentHost.Content =
                new DocumentAttachmentPanel(
                    viewModel.WeldId,
                    "Weld");
        }

        private void ExportPdf_Click(
            object sender,
                RoutedEventArgs e)
        {
            if (DataContext
                is not WeldDossierViewModel vm)
            {
                return;
            }

            var service =
                new WeldDossierPdfService();

            var exportModel =
                new WeldDossierExportModel
            {
                NdtRows =
                {
                    new NdtHistoryRow
                    {
                        Method = "RT",
                        ReportNumber = "RT-001",
                        Result = vm.NdtStatus,
                        Technician = "QA Inspector",
                        Date = DateTime.Now.ToString("yyyy-MM-dd")
                    }
                },

                NcrRows =
                {
                    new NcrHistoryRow
                    {
                        NcrNumber = "NCR-001",
                        Description = "Porosity detected",
                        Status = "Closed",
                        CorrectiveAction = "Excavate and repair",
                        Closed = "Yes"
                    }
                },

                RepairRows =
                {
                    new RepairHistoryRow
                    {
                        RepairNumber = "R1",
                        Reason = "Porosity",
                        RepairWps = "RWPS-001",
                        Welder = vm.WelderNumber,
                        Result = "Accepted",
                        Status = "Closed"
                    }
                },

                HoldPointRows =
                {
                    new HoldPointSignatureRow
                    {
                        HoldPoint = "Fit-Up Inspection",
                        Approver = "QA Inspector",
                        Role = "QA",
                        ApprovedDate =
                            DateTime.Now.ToString("yyyy-MM-dd"),
                        Status = "Approved"
                    },

                        new HoldPointSignatureRow
                    {
                        HoldPoint = "Final Visual Inspection",
                        Approver = "QC Supervisor",
                        Role = "QC",
                        ApprovedDate =
                            DateTime.Now.ToString("yyyy-MM-dd"),
                        Status = "Approved"
                    }
                },

                WeldSummary =
                {
                    new WeldSummaryRow
                    {
                        WeldNumber =
                            vm.WeldNumber,

                        Wps =
                            vm.WpsNumber,

                        Welder =
                            vm.WelderNumber,

                        NdtStatus =
                            vm.NdtStatus,

                        Repairs =
                            vm.RepairCount,

                        WorkflowStatus =
                            vm.WorkflowStatus
                    }
                },

                AttachmentRows =
                {
                    new AttachmentIndexRow
                    {
                        FileName = "RT_Report_001.pdf",
                        Category = "RT Report",
                        UploadedBy = "QA Inspector",
                        UploadedDate =
                            DateTime.Now.ToString("yyyy-MM-dd")
                    },

                    new AttachmentIndexRow
                    {
                        FileName = "Repair_Photo.jpg",
                        Category = "Repair Evidence",
                        UploadedBy = "QC Supervisor",
                        UploadedDate =
                            DateTime.Now.ToString("yyyy-MM-dd")
                    }
                },

                    QaSignoffs =
                    {
                        new QaSignoffRow
                        {
                            Role = "QA Inspector",
                            Name = "John Smith",
                            Status = "Approved",
                            Date =
                                DateTime.Now.ToString("yyyy-MM-dd"),
                            Signature = "SIGNED"
                        },

                        new QaSignoffRow
                        {
                            Role = "QC Supervisor",
                            Name = "Jane Doe",
                            Status = "Approved",
                            Date =
                                DateTime.Now.ToString("yyyy-MM-dd"),
                            Signature = "SIGNED"
                        },

                        new QaSignoffRow
                        {
                            Role = "Client Inspector",
                            Name = "Pending",
                            Status = "Pending",
                            Date = "",
                            Signature = ""
                        }
                    },

            };

            service.Export(exportModel);
        }
    }
}