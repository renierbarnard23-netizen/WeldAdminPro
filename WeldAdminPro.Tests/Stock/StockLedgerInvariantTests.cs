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
}
