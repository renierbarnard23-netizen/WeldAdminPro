using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ExecutiveProjectRiskViewModel : ObservableObject
	{
		private readonly ProjectRiskService _riskService;

		[ObservableProperty]
		private ObservableCollection<ProjectRiskModel> projects = new();

		public ExecutiveProjectRiskViewModel()
		{
			_riskService = new ProjectRiskService();
			Load();
		}

		private void Load()
		{
			var data = _riskService.GetProjectRiskSummary();

			Projects = new ObservableCollection<ProjectRiskModel>(data);
		}
	}
}