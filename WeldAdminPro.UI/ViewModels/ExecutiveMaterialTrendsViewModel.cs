using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ExecutiveMaterialTrendsViewModel : ObservableObject
	{
		private readonly MaterialTrendService _trendService;

		[ObservableProperty]
		private ObservableCollection<MaterialTrendModel> materials = new();

		public ExecutiveMaterialTrendsViewModel()
		{
			_trendService = new MaterialTrendService();
			Load();
		}

		private void Load()
		{
			var data = _trendService.GetMaterialTrends();

			Materials = new ObservableCollection<MaterialTrendModel>(data);
		}
	}
}