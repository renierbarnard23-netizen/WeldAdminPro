using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Services;

namespace WeldAdminPro.UI.Converters
{
    public class RoleToVisibilityConverter
        : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (parameter == null)
            {
                return Visibility.Collapsed;
            }

            var requiredRole =
                Enum.Parse<SystemRole>(
                    parameter.ToString()!);

            return CurrentUserContext.Role >= requiredRole
                ? Visibility.Visible
                : Visibility.Collapsed;
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
