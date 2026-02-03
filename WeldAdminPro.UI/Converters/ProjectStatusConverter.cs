using System;
using System.Globalization;
using System.Windows.Data;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.UI.Converters
{
	public class ProjectStatusConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not Project project)
				return string.Empty;

			var today = DateTime.Today;

			if (project.StartDate == null)
				return "Unscheduled";

			if (project.StartDate > today)
				return "Planned";

			if (project.EndDate != null && project.EndDate < today)
				return "Completed";

			return "Active";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
