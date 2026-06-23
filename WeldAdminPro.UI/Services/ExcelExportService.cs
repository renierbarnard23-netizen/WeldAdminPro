using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.Diagnostics;

namespace WeldAdminPro.UI.Services
{
    public class ExcelExportService
    {
        public Task ExportProjectsAsync<T>(IEnumerable<T> rows, string filename)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            var list = rows.ToList();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Projects");

            var props = typeof(T).GetProperties();

            // Header row (1)
            for (int c = 0; c < props.Length; c++)
            {
                var header = props[c].Name;
                var hcell = ws.Cell(1, c + 1);
                hcell.Value = header;
                hcell.Style.Font.Bold = true;
                hcell.Style.Fill.BackgroundColor = XLColor.FromHtml("#EEEFF2");
                hcell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                hcell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }

            // Data rows start at row 2
            for (int r = 0; r < list.Count; r++)
            {
                var item = list[r];
                for (int c = 0; c < props.Length; c++)
                {
                    var prop = props[c];
                    var raw = prop.GetValue(item);

                    var cell = ws.Cell(r + 2, c + 1);

                    // Typed assignment: use SetValue<T> or explicit conversions
                    if (raw is DateTime dt)
                    {
                        cell.SetValue(dt);
                        cell.Style.DateFormat.Format = "yyyy-MM-dd";
                    }
                    else if (raw is DateTimeOffset dto)
                    {
                        cell.SetValue(dto.DateTime);
                        cell.Style.DateFormat.Format = "yyyy-MM-dd";
                    }
                    else if (raw is int || raw is long || raw is short || raw is byte)
                    {
                        cell.SetValue(Convert.ToInt64(raw));
                    }
                    else if (raw is decimal || raw is double || raw is float)
                    {
                        cell.SetValue(Convert.ToDouble(raw));
                    }
                    else if (raw is bool b)
                    {
                        cell.SetValue(b);
                    }
                    else if (raw == null)
                    {
                        cell.SetValue(string.Empty);
                    }
                    else
                    {
                        // fallback — write as string
                        cell.SetValue(raw.ToString());
                    }
                }
            }

            // formatting niceties
            ws.Row(1).Style.Font.FontSize = 11;
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            var full = Path.GetFullPath(filename);
            wb.SaveAs(full);

            // optionally open file for user (ignore errors)
            try
            {
                Process.Start(new ProcessStartInfo(full) { UseShellExecute = true });
            }
            catch { /* ignore */ }

            return Task.CompletedTask;
        }
    }
}
