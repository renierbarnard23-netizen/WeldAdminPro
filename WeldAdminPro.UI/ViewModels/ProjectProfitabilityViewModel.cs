using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ProjectProfitabilityViewModel : ObservableObject
	{
		private readonly ProjectProfitabilityService _service;

		[ObservableProperty]
		private ObservableCollection<ProjectProfitability> projects = new();

		public ProjectProfitabilityViewModel()
		{
			_service = new ProjectProfitabilityService();
			Load();
		}

		private void Load()
		{
			var data = _service.GetProjectProfitability();

			Projects = new ObservableCollection<ProjectProfitability>(data);
		}
	}
}