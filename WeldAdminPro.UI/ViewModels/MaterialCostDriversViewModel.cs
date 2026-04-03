using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class MaterialCostDriversViewModel : ObservableObject
	{
		private readonly MaterialCostAnalysisService _service;

		[ObservableProperty]
		private ObservableCollection<MaterialCostDriver> costDrivers = new();

		public MaterialCostDriversViewModel()
		{
			_service = new MaterialCostAnalysisService();

			LoadData();
		}

		private void LoadData()
		{
			var results = _service.GetTopMaterialCostDrivers();

			CostDrivers = new ObservableCollection<MaterialCostDriver>(results);
		}
	}
}