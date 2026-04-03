using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class PurchaseOrderReviewViewModel : ObservableObject
	{
		private readonly PurchaseOrderRepository _repository;

		public PurchaseOrder PurchaseOrder { get; }

		public ObservableCollection<PurchaseOrderLine> Lines { get; }

		public decimal TotalAmount =>
			Lines.Sum(l => l.Quantity * l.UnitCost);

		public event Action? RequestClose;

		public PurchaseOrderReviewViewModel(PurchaseOrder po)
		{
			_repository = new PurchaseOrderRepository();

			PurchaseOrder = po;

			Lines = new ObservableCollection<PurchaseOrderLine>(po.Lines ?? new());
		}

		[RelayCommand]
		private void Save()
		{
			// Convert ObservableCollection → List
			PurchaseOrder.Lines = Lines.ToList();

			// Recalculate totals properly
			foreach (var line in PurchaseOrder.Lines)
			{
				line.LineTotal = line.Quantity * line.UnitCost;
			}

			PurchaseOrder.TotalAmount =
				PurchaseOrder.Lines.Sum(l => l.LineTotal);

			_repository.Save(PurchaseOrder);

			RequestClose?.Invoke();
		}

		[RelayCommand]
		private void Cancel()
		{
			RequestClose?.Invoke();
		}
	}
}
