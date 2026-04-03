using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WeldAdminPro.Core.Execution;

namespace WeldAdminPro.UI.Converters
{
	public class BlockReasonToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not BlockReason reason)
				return Brushes.Transparent;

			return reason switch
			{
				BlockReason.NoStock => new SolidColorBrush(Color.FromArgb(80, 255, 0, 0)),       // 🔴 Red
				BlockReason.InsufficientStock => new SolidColorBrush(Color.FromArgb(80, 255, 165, 0)), // 🟠 Orange
				BlockReason.DependencyNotMet => new SolidColorBrush(Color.FromArgb(80, 255, 215, 0)),  // 🟡 Yellow
				BlockReason.NotScheduled => new SolidColorBrush(Color.FromArgb(60, 200, 200, 200)),   // ⚪ Grey
				BlockReason.None => new SolidColorBrush(Color.FromArgb(60, 0, 200, 0)),              // 🟢 Green
				_ => Brushes.Transparent
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}