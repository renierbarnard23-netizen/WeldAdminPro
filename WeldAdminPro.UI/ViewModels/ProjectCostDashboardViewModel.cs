using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ProjectCostDashboardViewModel : ObservableObject
	{
		private readonly ProjectCostingService _service;

		[ObservableProperty]
		private ObservableCollection<ProjectCostSummary> projectCosts = new();

		public IRelayCommand LoadCommand { get; }

		public ProjectCostDashboardViewModel()
		{
			_service = new ProjectCostingService();

			LoadCommand = new RelayCommand(LoadData);

			LoadData();
		}

		private void LoadData()
		{
			var results = _service.GetProjectCostSummary();

			ProjectCosts = new ObservableCollection<ProjectCostSummary>(results);
		}
	}
}