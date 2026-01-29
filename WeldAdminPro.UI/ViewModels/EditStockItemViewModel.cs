using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

public partial class EditStockItemViewModel : ObservableObject
{
	private readonly StockRepository _repo;
	private readonly StockItem _item;

	public event Action? RequestClose;
	public event Action? ItemUpdated;

	[ObservableProperty] private string itemCode;
	[ObservableProperty] private string description;
	[ObservableProperty] private int quantity;
	[ObservableProperty] private string unit;
	[ObservableProperty] private decimal? minLevel;
	[ObservableProperty] private decimal? maxLevel;
	[ObservableProperty] private string category;

	public IRelayCommand SaveCommand { get; }
	public IRelayCommand CancelCommand { get; }

	public EditStockItemViewModel(StockItem item)
	{
		_repo = new StockRepository();
		_item = item;

		itemCode = item.ItemCode;
		description = item.Description;
		quantity = item.Quantity;
		unit = item.Unit;
		minLevel = item.MinLevel;
		maxLevel = item.MaxLevel;
		category = item.Category;

		SaveCommand = new RelayCommand(Save);
		CancelCommand = new RelayCommand(() => RequestClose?.Invoke());
	}

	private void Save()
	{
		_item.Description = Description;
		_item.Quantity = Quantity;
		_item.Unit = Unit;
		_item.MinLevel = MinLevel;
		_item.MaxLevel = MaxLevel;
		_item.Category = Category;

		_repo.Update(_item);

		ItemUpdated?.Invoke();
		RequestClose?.Invoke();
	}
}
