using System;

namespace WeldAdminPro.Core.Services
{
	public static class CategoryChangeNotifier
	{
		public static event Action? CategoriesChanged;

		public static void Notify()
		{
			CategoriesChanged?.Invoke();
		}
	}
}
