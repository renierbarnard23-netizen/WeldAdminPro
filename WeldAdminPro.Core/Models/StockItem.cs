using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeldAdminPro.Core.Models
{
	public class StockItem : INotifyPropertyChanged
	{
		public Guid Id { get; set; }

		public string ItemCode { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		// =========================
		// QUANTITY (Reactive)
		// =========================
		private int _quantity;
		public int Quantity
		{
			get => _quantity;
			set
			{
				if (_quantity != value)
				{
					_quantity = value;
					OnPropertyChanged();
					OnPropertyChanged(nameof(IsOutOfStock));
					OnPropertyChanged(nameof(IsLowStock));
					OnPropertyChanged(nameof(Status));
					OnPropertyChanged(nameof(TotalStockValue));
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
					OnPropertyChanged();
					OnPropertyChanged(nameof(IsLowStock));
					OnPropertyChanged(nameof(Status));
				}
			}
		}

		private decimal? _maxLevel;
		public decimal? MaxLevel
		{
			get => _maxLevel;
			set
			{
				if (_maxLevel != value)
				{
					_maxLevel = value;
					OnPropertyChanged();
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
				}
			}
		}

		// =========================
		// FINANCIAL
		// =========================
		public decimal TotalStockValue => Quantity * AverageUnitCost;

		// =========================
		// STATUS LOGIC
		// =========================
		public bool IsOutOfStock => Quantity <= 0;

		public bool IsLowStock =>
			MinLevel.HasValue &&
			Quantity > 0 &&
			Quantity <= MinLevel.Value;

		public StockStatus Status
		{
			get
			{
				if (IsOutOfStock)
					return StockStatus.Out;

				if (IsLowStock)
					return StockStatus.Low;

				return StockStatus.Normal;
			}
		}

		// =========================
		// PROPERTY CHANGED
		// =========================
		public event PropertyChangedEventHandler? PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string? name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
