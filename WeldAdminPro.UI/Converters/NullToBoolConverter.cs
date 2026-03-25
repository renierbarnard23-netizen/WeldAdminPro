using System;
using System.Globalization;
using System.Windows.Data;

namespace WeldAdminPro.UI.Converters
{
	public class NullToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// TRUE = enabled (no block)
			// FALSE = disabled (blocked)
			return string.IsNullOrEmpty(value?.ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}