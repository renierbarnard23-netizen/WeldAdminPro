using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Spreadsheet;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ExecutiveStockForecastViewModel : ObservableObject
	{
		private readonly StockForecastService _service;

		[ObservableProperty]
		private ObservableCollection<StockForecastModel> items = new();

		public ExecutiveStockForecastViewModel()
		{
			_service = new StockForecastService();
			Load();
		}

		private void Load()
		{
			var data = _service.GetStockForecast();
			Items = new ObservableCollection<StockForecastModel>(data);
		}
	}
}