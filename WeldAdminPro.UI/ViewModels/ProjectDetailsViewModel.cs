using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ProjectDetailsViewModel : ObservableObject
	{
		private readonly IProjectRepository _projectRepository;
		private readonly StockRepository _stockRepository;
		private readonly ProjectStockUsageRepository _usageRepository;

		public Project Project { get; }

		public IReadOnlyList<ProjectStatus> Statuses { get; }

		public ObservableCollection<StockItem> StockItems { get; }

		[ObservableProperty]
		private StockItem? selectedStockItem;

		[ObservableProperty]
		private decimal issueQuantity;

		[ObservableProperty]
		private string issuedBy = string.Empty;

		public event Action? RequestClose;

		public ProjectDetailsViewModel(Project project)
		{
			Project = project;

			_projectRepository = new ProjectRepository();
			_stockRepository = new StockRepository();
			_usageRepository = new ProjectStockUsageRepository();

			Statuses = Enum.GetValues(typeof(ProjectStatus))
						   .Cast<ProjectStatus>()
						   .ToList();

			StockItems = new ObservableCollection<StockItem>(
				_stockRepository.GetAll()
			);
		}

		// 🔒 Phase 8 rule: only Completed is locked
		public bool IsEditable => Project.Status != ProjectStatus.Completed;

		public ProjectStatus Status
		{
			get => Project.Status;
			set
			{
				if (Project.Status != value)
				{
					Project.Status = value;
					OnPropertyChanged(nameof(Status));
					OnPropertyChanged(nameof(IsEditable));
				}
			}
		}

		[RelayCommand]
		private void Save()
		{
			_projectRepository.Update(Project);
			RequestClose?.Invoke();
		}

		[RelayCommand]
		private void IssueStock()
		{
			if (!IsEditable)
				return;

			if (SelectedStockItem == null || IssueQuantity <= 0)
				return;

			var usage = new ProjectStockUsage
			{
				ProjectId = Project.Id,
				StockItemId = SelectedStockItem.Id,
				Quantity = IssueQuantity,
				IssuedBy = IssuedBy
			};

			_usageRepository.Add(usage);

			// Reset fields
			SelectedStockItem = null;
			IssueQuantity = 0;
			IssuedBy = string.Empty;
		}

		[RelayCommand]
		private void Cancel()
		{
			RequestClose?.Invoke();
		}
	}
}
