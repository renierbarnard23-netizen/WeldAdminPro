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

			// -------------------------------------------------
			// 1️⃣ Derive Executive Metrics From Existing Data
			// -------------------------------------------------

			int totalItems = analytics.ItemSummaries.Count;

			double deadStockPercent =
				totalItems > 0
					? (double)analytics.DeadStockCount / totalItems * 100
					: 0;

			int itemsBelow30Days =
				analytics.ItemSummaries.Count(x =>
					x.DaysUntilStockout > 0 && x.DaysUntilStockout < 30);

			int reorderRequiredCount =
				analytics.ItemSummaries.Count(x =>
					x.ReorderRiskLevel == "High" ||
					x.ReorderRiskLevel == "Critical-A");

			int highRiskHighValueOverlap =
				analytics.ItemSummaries.Count(x =>
					x.ABCClass == "A" &&
					(x.ReorderRiskLevel == "High" ||
					 x.ReorderRiskLevel == "Critical-A"));

			int criticalAItems = analytics.CriticalACount;

			// Deterministic Health Score (v1 baseline formula)
			int inventoryHealthScore =
				100
				- (analytics.CriticalACount * 10)
				- (analytics.HighRiskCount * 5)
				- (int)deadStockPercent;

			if (inventoryHealthScore < 0)
				inventoryHealthScore = 0;

			// -------------------------------------------------
			// 2️⃣ Evaluate Severity
			// -------------------------------------------------

			var evaluator = new ExecutiveSeverityEvaluator(options);
			var builder = new ExecutiveSummaryBuilder();

			var healthSeverity = evaluator.EvaluateHealth(
				inventoryHealthScore,
				analytics.CriticalACount,
				deadStockPercent);

			var capitalSeverity = evaluator.EvaluateCapital(
				(double)analytics.CapitalLockedPercentage,
				highRiskHighValueOverlap,
				deadStockPercent);

			var reorderSeverity = evaluator.EvaluateReorder(
				itemsBelow30Days,
				criticalAItems,
				reorderRequiredCount);

			// -------------------------------------------------
			// 3️⃣ Build Summary Blocks
			// -------------------------------------------------

			var healthBlock = builder.BuildHealthBlock(
				healthSeverity,
				inventoryHealthScore,
				analytics.CriticalACount,
				deadStockPercent);

			var capitalBlock = builder.BuildCapitalBlock(
				capitalSeverity,
				(double)analytics.CapitalLockedPercentage,
				highRiskHighValueOverlap,
				deadStockPercent);

			var reorderBlock = builder.BuildReorderBlock(
				reorderSeverity,
				itemsBelow30Days,
				criticalAItems,
				reorderRequiredCount);

			// -------------------------------------------------
			// 4️⃣ Generate PDF
			// -------------------------------------------------

			Document.Create(container =>
			{
				container.Page(page =>
				{
					page.Margin(40);

					// =============================
					// HEADER
					// =============================
					page.Header().Column(header =>
					{
						header.Item()
							.Text("Executive Inventory Risk Report")
							.FontSize(22)
							.Bold();

						header.Item()
							.Text($"Generated {DateTime.Now:yyyy-MM-dd HH:mm}")
							.FontSize(10)
							.FontColor(Colors.Grey.Medium);
					});

					// =============================
					// CONTENT
					// =============================
					page.Content().Column(column =>
					{
						column.Spacing(15);

						RenderSummaryBlock(column, healthBlock);
						RenderSummaryBlock(column, capitalBlock);
						RenderSummaryBlock(column, reorderBlock);

						column.Item().PaddingTop(15).Text("Top Risk Items").Bold();

						var topRisk = analytics.ItemSummaries
							.Where(x => x.ReorderRiskLevel == "Critical-A"
									 || x.ReorderRiskLevel == "High")
							.Take(10)
							.ToList();

						foreach (var item in topRisk)
						{
							column.Item().Text(
								$"{item.ItemCode} | {item.ReorderRiskLevel} | Days Left: {item.DaysUntilStockout}");
						}
					});

					// =============================
					// FOOTER
					// =============================
					page.Footer()
						.AlignCenter()
						.Text("WeldAdmin Pro – Executive Intelligence Module")
						.FontSize(9)
						.FontColor(Colors.Grey.Medium);
				});
			})
			.GeneratePdf(filePath);
		}

		// =====================================================
		// Summary Block Renderer
		// =====================================================

		private void RenderSummaryBlock(
			ColumnDescriptor column,
			ExecutiveSummaryBlock block)
		{
			column.Item().Column(inner =>
			{
				inner.Spacing(5);

				inner.Item().Row(row =>
				{
					row.RelativeItem().Text(block.Title).Bold().FontSize(14);

					row.ConstantItem(110)
						.Background(GetSeverityColor(block.Severity))
						.PaddingVertical(5)
						.AlignCenter()
						.Text(block.Severity.ToString())
						.FontColor(Colors.White)
						.Bold();
				});

				inner.Item().Text(block.Paragraph);

				inner.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
			});
		}

		private string GetSeverityColor(ExecutiveSeverityLevel severity)
		{
			return severity switch
			{
				ExecutiveSeverityLevel.Stable => Colors.Green.Darken2,
				ExecutiveSeverityLevel.Moderate => Colors.Orange.Darken2,
				ExecutiveSeverityLevel.AtRisk => Colors.Red.Darken2,
				_ => Colors.Grey.Medium
			};
		}
	}
}