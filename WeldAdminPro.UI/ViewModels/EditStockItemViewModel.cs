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

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(() => RequestClose?.Invoke());
    }

    private void Save()
    {
        _item.Description = Description;
        _item.Quantity = Quantity;
        _item.Unit = Unit;

        _repo.Update(_item);

        ItemUpdated?.Invoke();
        RequestClose?.Invoke();
    }
}
