using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeldAdminPro.Core.Models
{
	public enum SmartStockStatus
	{
		Critical,          // Quantity = 0
		ReorderRequired,   // Quantity <= MinLevel
		BelowTarget,       // Quantity < MaxLevel
		Healthy            // Quantity >= MaxLevel
	}

	public class StockItem : INotifyPropertyChanged
	{
		public Guid Id { get; set; }
		public string SKU { get; set; } = string.Empty;

		public string Barcode { get; set; } = string.Empty;

		public string ItemCode { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;
		public int MinimumStockLevel { get; set; } = 0;

		// =========================
		// QUANTITY (Reactive)
		// =========================
		private double _quantity;
		public double Quantity
		{
			get => _quantity;
			set
			{
				if (_quantity != value)
				{
					_quantity = value;
					RaiseAllStockSignals();
				}
			}
		}

		public string Unit { get; set; } = string.Empty;

		// =========================
		// MIN LEVEL (Reactive)
		// =========================
		private decimal? _minLevel;
		public decimal? MinLevel
		{
			get => _minLevel;
			set
			{
				if (_minLevel != value)
				{
					_minLevel = value;
					RaiseAllStockSignals();
				}
			}
		}

		// =========================
		// MAX LEVEL (Reactive)
		// =========================
		private decimal? _maxLevel;
		public decimal? MaxLevel
		{
			get => _maxLevel;
			set
			{
				if (_maxLevel != value)
				{
					_maxLevel = value;
					RaiseAllStockSignals();
				}
			}
		}

		public string Category { get; set; } = "Uncategorised";

		// =========================
		// AVERAGE UNIT COST (Reactive)
		// =========================
		private decimal _averageUnitCost;
		public decimal AverageUnitCost
		{
			get => _averageUnitCost;
			set
			{
				if (_averageUnitCost != value)
				{
					_averageUnitCost = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(TotalStockValue));
					OnPropertyChanged(nameof(StockValueRisk));
				}
			}
		}

		// =========================
		// FINANCIAL
		// =========================
		public decimal TotalStockValue => (decimal)Quantity * AverageUnitCost;

		// If item is below MinLevel, this is financial exposure
		public decimal StockValueRisk =>
			IsReorderRequired
				? SuggestedReorderQuantity * AverageUnitCost
				: 0;

		// =========================
		// SMART STATUS LOGIC
		// =========================

		public bool IsCritical => Quantity <= 0;

		public bool IsReorderRequired =>
			MinLevel.HasValue &&
			(decimal)Quantity <= MinLevel.Value;

		public bool IsBelowTarget =>
			MaxLevel.HasValue &&
			(decimal)Quantity < MaxLevel.Value &&
			!IsReorderRequired;

		public SmartStockStatus SmartStatus
		{
			get
			{
				if (IsCritical)
					return SmartStockStatus.Critical;

				if (IsReorderRequired)
					return SmartStockStatus.ReorderRequired;

				if (IsBelowTarget)
					return SmartStockStatus.BelowTarget;

				return SmartStockStatus.Healthy;
			}
		}

		// =========================
		// SMART REORDER ENGINE
		// =========================

		public int SuggestedReorderQuantity
		{
			get
			{
				if (!MaxLevel.HasValue)
					return 0;

				var suggested = (int)Math.Ceiling(MaxLevel.Value - (decimal)Quantity);

				return suggested > 0 ? suggested : 0;
			}
		}

		public int SuggestedSmartReorderQuantity(decimal avgDailyUsage)
		{
			if (avgDailyUsage <= 0)
				return SuggestedReorderQuantity;

			var demandDuringLeadTime =
				avgDailyUsage * SupplierLeadTimeDays;

			var safetyStock =
				avgDailyUsage * SafetyStockDays;

			var requiredStock =
				demandDuringLeadTime + safetyStock;

			var suggested =
				(int)Math.Ceiling(requiredStock - (decimal)Quantity);

			return suggested > 0 ? suggested : 0;
		}

		public bool NeedsReorder =>
			SuggestedReorderQuantity > 0;

		public int SupplierLeadTimeDays { get; set; } = 7;
		public int SafetyStockDays { get; set; } = 3;

		// =========================
		// PROPERTY CHANGED
		// =========================
		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string? name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private void RaiseAllStockSignals()
		{
			OnPropertyChanged(nameof(Quantity));
			OnPropertyChanged(nameof(IsCritical));
			OnPropertyChanged(nameof(IsReorderRequired));
			OnPropertyChanged(nameof(IsBelowTarget));
			OnPropertyChanged(nameof(SmartStatus));
			OnPropertyChanged(nameof(SuggestedReorderQuantity));
			OnPropertyChanged(nameof(NeedsReorder));
			OnPropertyChanged(nameof(TotalStockValue));
			OnPropertyChanged(nameof(StockValueRisk));
		}
	}
}
