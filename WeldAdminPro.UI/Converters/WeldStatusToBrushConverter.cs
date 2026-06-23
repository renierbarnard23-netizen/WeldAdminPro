using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.UI.Converters
{
    public class WeldStatusToBrushConverter
        : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is not WeldWorkflowStatus status)
            {
                return Brushes.White;
            }

            return value?.ToString() switch
            {
                "Accepted" =>
                    Brushes.LightGreen,

                "NdtPending" =>
                    Brushes.Khaki,

                "NdtInProgress" =>
                    Brushes.LightBlue,

                "RepairRequired" =>
                    Brushes.OrangeRed,

                "UnderRepair" =>
                    Brushes.Orange,

                "AwaitingReinspection" =>
                    Brushes.DeepSkyBlue,

                "ReinspectionRequired" =>
                    Brushes.DeepSkyBlue,

                "Released" =>
                    Brushes.LightSteelBlue,

                "TurnoverReady" =>
                    Brushes.MediumPurple,

                "Closed" =>
                    Brushes.LightGray,

                _ =>
                    Brushes.White
            };
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}