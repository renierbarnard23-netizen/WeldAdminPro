using System;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Core.Configuration;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Services;

namespace WeldAdminPro.Core.Reporting
{
	public class ExecutiveReportService
	{
		public void GenerateExecutiveReport(
			StockAnalyticsResult analytics,
			ExecutiveSeverityOptions options,
			string filePath)
		{
			QuestPDF.Settings.License = LicenseType.Community;

			int totalItems = analytics.ItemSummaries.Count;

			double deadStockPercent =
				totalItems > 0
					? (double)analytics.DeadStockCount / totalItems * 100
					: 0;

			bool extremeCapitalLock = analytics.CapitalLockedPercentage >= 75;

			decimal cValuePercent =
				analytics.TotalInventoryValue > 0
					? Math.Round((analytics.CValue / analytics.TotalInventoryValue) * 100m, 1)
					: 0;

			bool portfolioFrozen = cValuePercent >= 80;

			int itemsBelow30Days =
				analytics.ItemSummaries.Count(x =>
					x.DaysUntilStockout > 0 && x.DaysUntilStockout < 30);

			int reorderRequiredCount =
				analytics.ItemSummaries.Count(x =>
					x.ReorderRiskLevel == "High" ||
					x.ReorderRiskLevel == "Critical-A");

			int inventoryHealthScore =
				100
				- (analytics.CriticalACount * 10)
				- (analytics.HighRiskCount * 5)
				- (int)deadStockPercent;

			if (inventoryHealthScore < 0)
				inventoryHealthScore = 0;

			var evaluator = new ExecutiveSeverityEvaluator(options);
			var builder = new ExecutiveSummaryBuilder();

			var healthSeverity = evaluator.EvaluateHealth(
				inventoryHealthScore,
				analytics.CriticalACount,
				deadStockPercent);

			var capitalSeverity = evaluator.EvaluateCapital(
				(double)analytics.CapitalLockedPercentage,
				0,
				deadStockPercent);

			var reorderSeverity = evaluator.EvaluateReorder(
				itemsBelow30Days,
				analytics.CriticalACount,
				reorderRequiredCount);

			if (extremeCapitalLock && portfolioFrozen)
			{
				healthSeverity = ExecutiveSeverityLevel.Critical;
				capitalSeverity = ExecutiveSeverityLevel.Critical;
			}

			var healthBlock = builder.BuildHealthBlock(
				healthSeverity,
				inventoryHealthScore,
				analytics.CriticalACount,
				deadStockPercent);

			var capitalBlock = builder.BuildCapitalBlock(
				capitalSeverity,
				(double)analytics.CapitalLockedPercentage,
				0,
				deadStockPercent);

			var reorderBlock = builder.BuildReorderBlock(
				reorderSeverity,
				itemsBelow30Days,
				analytics.CriticalACount,
				reorderRequiredCount);

			var document = Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Margin(40);

					// HEADER
					page.Header().Element(header =>
					{
						header.Background("#1F2933")
							.Padding(20)
							.Column(column =>
							{
								column.Item().Text("TETRACUBE PTY LTD")
									.FontSize(14).SemiBold().FontColor(Colors.White);

								column.Item().Text("WeldAdmin Pro")
									.FontSize(20).Bold().FontColor(Colors.White);

								column.Item().Text("Executive Inventory Risk Report")
									.FontSize(12).FontColor("#D1D5DB");

								column.Item().Text(
									$"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
									.FontSize(9).FontColor("#9CA3AF");
							});
					});

					page.Content().Column(column =>
					{
						column.Spacing(15);

						if (healthSeverity == ExecutiveSeverityLevel.Critical)
						{
							column.Item().Background(Colors.Black)
								.Padding(12)
								.Text("EXECUTIVE ALERT: STRUCTURAL CAPITAL FAILURE DETECTED")
								.FontColor(Colors.White)
								.Bold();
						}

						// CAPITAL BLOCK (with KPI grid inside)
						column.Item().Border(1)
							.BorderColor(Colors.Grey.Lighten1)
							.Padding(15)
							.Column(cap =>
							{
								cap.Spacing(10);

								cap.Item().Text("Capital Exposure Overview")
									.FontSize(14).SemiBold();

								cap.Item().Text(
									$"Total Inventory Value: {analytics.TotalInventoryValue:C}")
									.FontSize(16).Bold();

								cap.Item().Text(
									$"Capital Locked: {analytics.CapitalLockedPercentage}% of total portfolio")
									.FontColor(
										analytics.CapitalLockedPercentage >= 20
											? Colors.Red.Darken2
											: Colors.Grey.Darken2);

								// KPI GRID
								cap.Item().PaddingTop(10).Table(table =>
								{
									table.ColumnsDefinition(columns =>
									{
										columns.RelativeColumn();
										columns.RelativeColumn();
									});

									void Cell(string title, string value)
									{
										table.Cell().Border(1)
											.BorderColor(Colors.Grey.Lighten2)
											.Padding(8)
											.Column(c =>
											{
												c.Item().Text(title)
													.FontSize(9)
													.FontColor(Colors.Grey.Darken1);
												c.Item().Text(value)
													.FontSize(13)
													.Bold();
											});
									}

									Cell("Inventory Health", $"{healthSeverity} ({inventoryHealthScore}/100)");
									Cell("Critical-A Items", analytics.CriticalACount.ToString());
									Cell("High Risk Items", analytics.HighRiskCount.ToString());
									Cell("Dead Stock Count", analytics.DeadStockCount.ToString());
								});
							});

						// ABC TABLE
						column.Item().Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn();
								columns.RelativeColumn();
								columns.RelativeColumn();
							});

							table.Cell().Text("Class").SemiBold();
							table.Cell().Text("Item Count").SemiBold();
							table.Cell().Text("% Portfolio Value").SemiBold();

							decimal totalValue = analytics.TotalInventoryValue;

							void Row(string label, int count, decimal value)
							{
								table.Cell().Text(label);
								table.Cell().Text(count.ToString());
								table.Cell().Text(
									totalValue > 0
										? Math.Round((value / totalValue) * 100m, 1) + "%"
										: "0%");
							}

							Row("A", analytics.AItemCount, analytics.AValue);
							Row("B", analytics.BItemCount, analytics.BValue);
							Row("C", analytics.CItemCount, analytics.CValue);
						});

						RenderSummaryBlock(column, healthBlock);
						RenderSummaryBlock(column, capitalBlock);
						RenderSummaryBlock(column, reorderBlock);

						// TOP RISK TABLE
						column.Item().PaddingTop(15)
							.Text("Top Risk Exposure")
							.FontSize(14)
							.Bold();

						var topRisk = analytics.ItemSummaries
							.Where(x => x.ReorderRiskLevel == "Critical-A"
									 || x.ReorderRiskLevel == "High")
							.OrderBy(x => x.DaysUntilStockout)
							.Take(10)
							.ToList();

						column.Item().Table(table =>
						{
							table.ColumnsDefinition(columns =>
							{
								columns.RelativeColumn(2);
								columns.RelativeColumn(1);
								columns.RelativeColumn(2);
								columns.RelativeColumn(1);
							});

							table.Cell().Text("Item Code").SemiBold();
							table.Cell().Text("ABC").SemiBold();
							table.Cell().Text("Risk Level").SemiBold();
							table.Cell().Text("Days Left").SemiBold();

							foreach (var item in topRisk)
							{
								table.Cell().Text(item.ItemCode);
								table.Cell().Text(item.ABCClass);
								table.Cell().Text(item.ReorderRiskLevel);
								table.Cell().Text(item.DaysUntilStockout.ToString());
							}
						});
					});

					page.Footer()
						.AlignCenter()
						.Text("WeldAdmin Pro – Executive Intelligence Module")
						.FontSize(9)
						.FontColor(Colors.Grey.Medium);
				});
			});

			document.GeneratePdf(filePath);
		}

		private void RenderSummaryBlock(ColumnDescriptor column, ExecutiveSummaryBlock block)
		{
			column.Item().Column(inner =>
			{
				inner.Item().Row(row =>
				{
					row.RelativeItem().Text(block.Title).Bold();
					row.ConstantItem(110)
						.Background(GetSeverityColor(block.Severity))
						.AlignCenter()
						.Text(block.Severity.ToString())
						.FontColor(Colors.White)
						.Bold();
				});

				inner.Item().Text(block.Paragraph);
				inner.Item().LineHorizontal(1);
			});
		}

		private string GetSeverityColor(ExecutiveSeverityLevel severity)
		{
			return severity switch
			{
				ExecutiveSeverityLevel.Stable => Colors.Green.Darken2,
				ExecutiveSeverityLevel.Moderate => Colors.Orange.Darken2,
				ExecutiveSeverityLevel.AtRisk => Colors.Red.Darken2,
				ExecutiveSeverityLevel.Critical => Colors.Black,
				_ => Colors.Grey.Medium
			};
		}
	}
}