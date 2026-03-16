using System.Collections.ObjectModel;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
	public class ProductionPlannerViewModel
	{
		public ObservableCollection<AIProductionRecommendation> Recommendations { get; set; }
			= new ObservableCollection<AIProductionRecommendation>();

		public void Load()
		{
			var planner = new ProductionAIPlannerService();

			var recs = planner.GetRecommendations();

			Recommendations.Clear();

			foreach (var r in recs)
				Recommendations.Add(r);
		}
	}
}