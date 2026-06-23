using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.UI.Converters
{
    public class HoldPointStatusToBrushConverter
        : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is not HoldPointStatus status)
            {
                return Brushes.White;
            }

            return status switch
            {
                HoldPointStatus.Pending =>
                    Brushes.Khaki,

                HoldPointStatus.Approved =>
                    Brushes.LightGreen,

                HoldPointStatus.Rejected =>
                    Brushes.IndianRed,

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