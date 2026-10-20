using ClosedXML.Excel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Services.Inventory;

namespace WeldAdminPro.Web.Services.Inventory;

public class InventoryExportService
{
    private readonly StockApplicationService _stockService;

    public InventoryExportService(
        StockApplicationService stockService)
    {
        _stockService = stockService;
    }

    public byte[] ExportStockWarnings()
    {
        var warnings = _stockService
            .GetStockItems()
            .Where(i =>
                i.SmartStatus == SmartStockStatus.Critical ||
                i.SmartStatus == SmartStockStatus.ReorderRequired)
            .ToList();

        using var workbook = new XLWorkbook();

        var worksheet =
            workbook.Worksheets.Add("Stock Warnings");

        worksheet.Cell(1, 1).Value = "Item Code";
        worksheet.Cell(1, 2).Value = "Description";
        worksheet.Cell(1, 3).Value = "Quantity";
        worksheet.Cell(1, 4).Value = "Unit";
        worksheet.Cell(1, 5).Value = "Category";
        worksheet.Cell(1, 6).Value = "Smart Status";
        worksheet.Cell(1, 7).Value = "Suggested Reorder Qty";

        worksheet
            .Range(1, 1, 1, 7)
            .Style
            .Font
            .Bold = true;

        var row = 2;

        foreach (var item in warnings)
        {
            worksheet.Cell(row, 1).Value = item.ItemCode;
            worksheet.Cell(row, 2).Value = item.Description;
            worksheet.Cell(row, 3).Value = item.Quantity;
            worksheet.Cell(row, 4).Value = item.Unit;
            worksheet.Cell(row, 5).Value = item.Category;
            worksheet.Cell(row, 6).Value = item.SmartStatus.ToString();
            worksheet.Cell(row, 7).Value = item.SuggestedReorderQuantity;

            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return stream.ToArray();
    }

    public bool HasStockWarnings()
    {
        return _stockService
            .GetStockItems()
            .Any(i =>
                i.SmartStatus == SmartStockStatus.Critical ||
                i.SmartStatus == SmartStockStatus.ReorderRequired);
    }
}
