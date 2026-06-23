using System;

namespace WeldAdminPro.Core.Quality
{
    public class WeldMapPoint
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProjectId { get; set; }

        public string WeldNumber { get; set; } = "";

        public double X { get; set; }
        public double Y { get; set; }
    }
}