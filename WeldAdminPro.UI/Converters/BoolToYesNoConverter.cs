using System;
using System.Globalization;
using System.Windows.Data;

namespace WeldAdminPro.UI.Converters
{
    public class BoolToYesNoConverter
        : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is bool b)
                return b ? "Yes" : "No";

            return "No";
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value?.ToString() == "Yes";
        }
    }
}