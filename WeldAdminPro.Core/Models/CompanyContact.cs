using System;

namespace WeldAdminPro.Core.Models
{
    public class CompanyContact
    {
        public Guid Id { get; set; } =
            Guid.NewGuid();

        public Guid CompanyId { get; set; }

        public string ContactName { get; set; } = "";

        public string Position { get; set; } = "";

        public string Telephone { get; set; } = "";

        public string Mobile { get; set; } = "";

        public string Email { get; set; } = "";

        public bool IsPrimary { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } =
            DateTime.UtcNow;
    }
}