using ScottPlot;
using ScottPlot.Plottables;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Reporting.Services
{
    public class WeldRepairChartService
    {
        public byte[] GenerateRepairStatusChart(
            List<Weld> welds)
        {
            var accepted =
                welds.Count(x =>
                    x.Status ==
                    WeldStatusType.Accepted);

            var repair =
                welds.Count(x =>
                    x.RepairCycle > 0);  

            var closed =
                welds.Count(x =>
                    x.Status ==
                    WeldStatusType.Closed);

            var pending =
                welds.Count(x =>
                    x.Status ==
                    WeldStatusType.Pending);

            double[] values =
            {
                accepted,
                repair,
                closed,
                pending
            };

            string[] labels =
            {
                "Accepted",
                "Repair",
                "Closed",
                "Pending"
            };

            var plot = new Plot();

            var pie = plot.Add.Pie(values);

            for (int i = 0; i < pie.Slices.Count; i++)
            {
                pie.Slices[i].Label =
                    $"{labels[i]} ({values[i]})";
            }

            plot.ShowLegend();

            plot.Title(
                "Weld Status Distribution");

            return plot.GetImageBytes(
                800,
                500,
                ImageFormat.Png);
        }
    }
}
