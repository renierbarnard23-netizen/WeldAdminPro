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

		public ObservableCollection<ProjectStockUsage> IssuedStockHistory { get; }

		[ObservableProperty]
		private StockItem? selectedStockItem;

		[ObservableProperty]
		private decimal issueQuantity;

		[ObservableProperty]
		private string issuedBy = string.Empty;

		public event Action? RequestClose;

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

		public ProjectDetailsViewModel(Project project)
		{
			Project = project;

			_projectRepository = new ProjectRepository();
			_stockRepository = new StockRepository();
			_usageRepository = new ProjectStockUsageRepository();

			Statuses = Enum.GetValues(typeof(ProjectStatus))
						   .Cast<ProjectStatus>()
						   .ToList();

			var stockItems = _stockRepository.GetAll();
			StockItems = new ObservableCollection<StockItem>(stockItems);

			var stockLookup = stockItems.ToDictionary(s => s.Id, s => s.Description);

			var history = _usageRepository.GetByProjectId(Project.Id);

			foreach (var h in history)
			{
				h.Notes = stockLookup.TryGetValue(h.StockItemId, out var desc)
					? desc
					: h.StockItemId.ToString();
			}

			IssuedStockHistory = new ObservableCollection<ProjectStockUsage>(history);
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
			if (!IsEditable || SelectedStockItem == null || IssueQuantity <= 0)
				return;

			var usage = new ProjectStockUsage
			{
				Id = Guid.NewGuid(),
				ProjectId = Project.Id,
				StockItemId = SelectedStockItem.Id,
				Quantity = IssueQuantity,
				IssuedBy = IssuedBy,
				IssuedOn = DateTime.UtcNow,
				Notes = SelectedStockItem.Description
			};

			_usageRepository.Add(usage);
			IssuedStockHistory.Insert(0, usage);

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
