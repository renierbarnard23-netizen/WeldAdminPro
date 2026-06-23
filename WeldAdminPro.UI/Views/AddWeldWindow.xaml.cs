using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Services;

using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.Views
{
    public partial class AddWeldWindow : Window
    {
        public Weld? Weld { get; private set; }

        private readonly WelderQualificationRepository _qualificationRepo;
        private readonly WpsRepository _wpsRepo;

        public AddWeldWindow(string weldNumber)
        {
            InitializeComponent();

            _qualificationRepo =
                new WelderQualificationRepository();

            _wpsRepo =
                new WpsRepository();

            WeldNumberTextBox.Text = weldNumber;

            // =========================
            // JOINT TYPES
            // =========================

            JointTypeComboBox.SelectedIndex = 0;

            // =========================
            // LOAD WPS
            // =========================

            var wpsList = _wpsRepo.GetAll();

            foreach (var wps in wpsList)
            {
                WpsComboBox.Items.Add(wps.WpsNumber);
            }

            if (WpsComboBox.Items.Count > 0)
            {
                WpsComboBox.SelectedIndex = 0;
            }

            // =========================
            // LOAD VALID WELDERS
            // =========================

            var welders =
                _qualificationRepo.GetAll()
                    .Where(w =>
                        w.IsActive &&
                        w.ExpiryDate >= DateTime.Today)
                    .GroupBy(w => w.WelderNumber)
                    .Select(g => g.First())
                    .OrderBy(w => w.WelderNumber)
                    .ToList();

            foreach (var welder in welders)
            {
                WelderComboBox.Items.Add(welder);
            }

            WelderComboBox.DisplayMemberPath =
                "WelderNumber";

            WelderComboBox.SelectedValuePath =
                "WelderNumber";

            WelderComboBox.SelectionChanged +=
                WelderComboBox_SelectionChanged;

            if (WelderComboBox.Items.Count > 0)
            {
                WelderComboBox.SelectedIndex = 0;
            }
        }

        private void WelderComboBox_SelectionChanged(
    object sender,
    SelectionChangedEventArgs e)
        {
            if (WelderComboBox.SelectedItem
                is not WeldAdminPro.Core.Quality.WelderQualification welder)
            {
                return;
            }

            // =====================================
            // LOAD ALL QUALIFICATIONS FOR WELDER
            // =====================================

            var qualifications =
                _qualificationRepo.GetAll()
                .Where(x =>
                    x.WelderNumber == welder.WelderNumber &&
                    x.ExpiryDate >= DateTime.Today)
                .ToList();

            var materials =
                string.Join(", ",
                    qualifications
                        .Select(x => x.MaterialGroup));

            // =====================================
            // MATERIAL GROUPS
            // =====================================

            MaterialGroupComboBox.Items.Clear();

            foreach (var material in qualifications
                .Select(x => x.MaterialGroup.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x))
            {
                MaterialGroupComboBox.Items.Add(material);
            }
                        
            if (MaterialGroupComboBox.Items.Count > 0)
            {
                MaterialGroupComboBox.SelectedIndex = 0;
            }

            // =====================================
            // POSITIONS
            // =====================================

            PositionComboBox.Items.Clear();

            foreach (var position in qualifications
                .Select(x => x.Position)
                .Distinct())
            {
                PositionComboBox.Items.Add(
                    new ComboBoxItem
                    {
                        Content = position
                    });
            }

            if (PositionComboBox.Items.Count > 0)
            {
                PositionComboBox.SelectedIndex = 0;
            }

            // =====================================
            // PROCESSES
            // =====================================

            ProcessComboBox.Items.Clear();

            foreach (var process in qualifications
                .Select(x => x.Process)
                .Distinct())
            {
                ProcessComboBox.Items.Add(
                    new ComboBoxItem
                    {
                        Content = process
                    });
            }

            if (ProcessComboBox.Items.Count > 0)
            {
                ProcessComboBox.SelectedIndex = 0;
            }

            // =====================================
            // THICKNESS INFO
            // =====================================

            var minThickness =
                qualifications.Min(x => x.ThicknessMin);

            var maxThickness =
                qualifications.Max(x => x.ThicknessMax);

            ThicknessTextBox.Text =
                minThickness.ToString("0.00");

            ThicknessTextBox.ToolTip =
                $"Qualified Range: " +
                $"{minThickness:0.00} - " +
                $"{maxThickness:0.00} mm";
        }

        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            var selectedWelder =
                WelderComboBox.SelectedItem
                as WeldAdminPro.Core.Quality.WelderQualification;

            if (selectedWelder == null)
            {
                MessageBox.Show(
                    "Select a welder");

                return;
            }

            var selectedWpsNumber =
                WpsComboBox.SelectedItem
                ?.ToString() ?? "";

            var wps =
                _wpsRepo.GetByNumber(
                    selectedWpsNumber);

            if (wps == null)
            {
                MessageBox.Show(
                    "WPS not found");

                return;
            }

            var thickness =
                double.TryParse(
                    ThicknessTextBox.Text,
                    out double t)
                    ? t
                    : 0;

            // =========================
            // CREATE WELD
            // =========================

            Weld = new Weld
            {
                Id = Guid.NewGuid(),

                WeldNumber =
                    WeldNumberTextBox.Text,

                JointNumber =
                    JointNumberTextBox.Text,

                MaterialSpecification =
                    MaterialTextBox.Text,

                Diameter =
                    double.TryParse(
                        DiameterTextBox.Text,
                        out var d)
                            ? d
                            : 0,

                DrawingNumber =
                    DrawingNumberTextBox.Text,

                JointType =
                    (JointTypeComboBox.SelectedItem
                        as ComboBoxItem)
                    ?.Content?.ToString()
                    ?? "Butt",

                WpsNumber =
                    selectedWpsNumber,

                WelderNumber =
                    selectedWelder.WelderNumber,

                MaterialHeat1 =
                    Heat1TextBox.Text,

                MaterialHeat2 =
                    Heat2TextBox.Text,

                Process =
                    (ProcessComboBox.SelectedItem
                        as ComboBoxItem)
                    ?.Content?.ToString()
                    ?? "",

                MaterialGroup =
                    MaterialGroupComboBox.SelectedItem
                        ?.ToString()
                        ?? "",

                Position =
    (PositionComboBox.SelectedItem
        as ComboBoxItem)
    ?.Content?.ToString()
    ?? "",

                Thickness =
                    thickness,

                Status =
                    Enum.TryParse<WeldStatusType>(
                        (StatusComboBox.SelectedItem as ComboBoxItem)
                        ?.Content?.ToString(),
                    out var status)
                        ? status
                        : WeldStatusType.Pending,

                NdtStatus = "Not Tested",

                CreatedDate =
                    DateTime.UtcNow
            };


            // =========================
            // GET ALL MATCHING QUALIFICATIONS
            // =========================

            var qualifications =
                _qualificationRepo.GetQualifications(
                    Weld.WelderNumber,
                    Weld.Process,
                    Weld.MaterialGroup,
                    Weld.Position);

            if (!qualifications.Any())
            {
                MessageBox.Show(
                    "Welder has no valid qualification");

                return;
            }

            // =========================
            // VALIDATE AGAINST ALL RANGES
            // =========================

            var validator =
                new WeldValidationService();

            bool valid = false;

            string validationMessage = "";

            foreach (var qualification in qualifications)
            {
                var result =
                    validator.Validate(
                    qualification,
                    wps,
                    Weld.Thickness);

                validationMessage =
                    string.Join(
                        Environment.NewLine,
                        result.Errors);

                if (result.IsValid)
                {
                    valid = true;

                    validationMessage = "";

                    break;
                }
            }

            Weld.IsValid = valid;

            Weld.ValidationMessage =
                validationMessage;

            if (!valid)
            {
                MessageBox.Show(
                    validationMessage);

                return;
            }

            DialogResult = true;

            Close();
        }

        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }
}