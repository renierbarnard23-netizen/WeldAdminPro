using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Procurement;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.Data.Services
{
	public class ProcurementSuggestionService
	{
		private readonly WorkOrderShortageDetectionService _shortageService;
		private readonly MaterialDemandForecastService _forecastService;

		public ProcurementSuggestionService()
		{
			_shortageService = new WorkOrderShortageDetectionService();
			_forecastService = new MaterialDemandForecastService();
		}

		public List<ProcurementSuggestion> GenerateSuggestions()
		{
			var shortages = _shortageService.DetectShortages();
			var forecasts = _forecastService.GenerateForecast();

			var suggestions = new List<ProcurementSuggestion>();

			foreach (var shortage in shortages)
			{
				var forecast = forecasts
					.FirstOrDefault(f => f.ItemCode == shortage.ItemCode);

				decimal suggestedQty = shortage.ShortageQuantity;

				if (forecast != null)
				{
					suggestedQty += forecast.SuggestedOrderQuantity;
				}

				suggestions.Add(new ProcurementSuggestion
				{
					ItemCode = shortage.ItemCode,
					Description = shortage.ItemName,
					CurrentStock = shortage.AvailableQuantity,
					RequiredQuantity = shortage.RequiredQuantity,
					SuggestedOrderQuantity = suggestedQty,
					PriorityScore = forecast?.PriorityScore ?? 50,
					Reason = "Work Order Shortage"
				});
			}

			return suggestions
				.OrderByDescending(s => s.PriorityScore)
				.ToList();
		}
	}
}