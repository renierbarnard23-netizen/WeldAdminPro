using System;
using System.Globalization;
using System.Windows.Data;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Quality.Services;

namespace WeldAdminPro.UI.Converters
{
    public class WelderStatusConverter : IValueConverter
    {
        private readonly WelderQualificationService _service = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return "";

                // 🔥 CRITICAL FIX
                if (value.GetType().FullName == "MS.Internal.NamedObject")
                    return "";

                if (value is not WelderQualification w)
                    return "";

                return _service.GetStatus(w) switch
                {
                    QualificationStatus.Valid => "✔",
                    QualificationStatus.ExpiringSoon => "⚠",
                    QualificationStatus.Expired => "❌",
                    _ => ""
                };
            }
            catch
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}