using System;

namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldHistoryEntry
    {
        public Guid Id { get; set; }

        public Guid WeldId { get; set; }

        public DateTime EventDate { get; set; }

        public string EventType { get; set; } = "";

        public string Description { get; set; } = "";

        public string UserName { get; set; } = "";

        public string StatusSnapshot { get; set; } = "";
    }
}