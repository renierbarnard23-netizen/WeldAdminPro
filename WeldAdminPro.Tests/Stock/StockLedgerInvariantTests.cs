using Xunit;
using WeldAdminPro.Core.Stock;
using WeldAdminPro.Core.Stock.Entities;
using WeldAdminPro.Core.Stock.Repositories;

public class StockLedgerInvariantTests
{
    [Fact]
    public void Last_Ledger_RunningBalance_Must_Match_StockItem_Quantity()
    {
        // Arrange
        var repo = TestStockRepositoryFactory.Create(); // use your test repo / in-memory DB

        var item = new StockItem
        {
            ItemCode = "TEST-001",
            Description = "Invariant Test Item",
            Quantity = 0
        };

        repo.StockItems.Add(item);
        repo.SaveChanges();

        // Act
        repo.AddStockIn(item.Id, 10, "Initial stock");
        repo.AddStockOut(item.Id, 3, "Usage");

        var reloadedItem = repo.StockItems.GetById(item.Id);
        var lastLedger = repo.StockLedger.GetLastEntry(item.Id);

        // Assert
        Assert.NotNull(lastLedger);
        Assert.Equal(reloadedItem.Quantity, lastLedger.RunningBalance);
    }
[Fact]
public void StockOut_Should_Be_Blocked_When_Quantity_Exceeds_Available()
{
    // Arrange
    var repo = TestStockRepositoryFactory.Create();

    var item = new StockItem
    {
        ItemCode = "UX-TEST",
        Description = "UX stock out test",
        Quantity = 5
    };

    repo.StockItems.Add(item);
    repo.SaveChanges();

    // Act + Assert
    Assert.Throws<InvalidOperationException>(() =>
    {
        repo.AddStockOut(item.Id, 10, "Attempt invalid stock out");
    });
}

[Fact]
public void Ledger_Should_Not_Allow_Negative_Running_Balance()
{
    // Arrange
    var repo = TestStockRepositoryFactory.Create();

    var item = new StockItem
    {
        ItemCode = "NEG-TEST",
        Description = "Negative balance test",
        Quantity = 5
    };

    repo.StockItems.Add(item);
    repo.SaveChanges();

    // Act
    repo.AddStockOut(item.Id, 10, "Attempt to overdraw");

    var lastLedger = repo.StockLedger.GetLastEntry(item.Id);

    // Assert
    Assert.NotNull(lastLedger);
    Assert.True(lastLedger.RunningBalance >= 0,
        "Ledger balance must never be negative");
}
}
