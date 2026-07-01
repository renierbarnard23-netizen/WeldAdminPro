using System;
using System.Collections.Generic;

namespace WeldAdminPro.Data.Services.ProductionEngine
{
    /*
    ==========================================================
    Production Engine Refresh Result

    Represents the outcome of a complete production refresh.

    The UI should consume this object rather than talking
    directly to individual services.
    ==========================================================
    */

    public class ProductionRefreshResult
    {
        public bool Success { get; set; } = true;

        public DateTime RefreshTime { get; set; }
            = DateTime.Now;

        public ProductionSnapshot Snapshot { get; set; }
            = new();

        public List<string> Messages { get; set; }
            = new();

        public TimeSpan Duration { get; set; }
    }
}