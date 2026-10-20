using System;

namespace WeldAdminPro.Core.Models
{
    public class Company
    {
        public Guid Id { get; set; } =
            Guid.NewGuid();

        public string CompanyName { get; set; } = "";

        public string CompanyCode { get; set; } = "";

        public string RegistrationNumber { get; set; } = "";

        public string VatNumber { get; set; } = "";

        public string VatStatus { get; set; } = "N/A";

        public string CompanyType { get; set; } = "Other";

        public string Telephone { get; set; } = "";

        public string Mobile { get; set; } = "";

        public string Email { get; set; } = "";

        public string Website { get; set; } = "";

        public string PhysicalAddress { get; set; } = "";

        public string PostalAddress { get; set; } = "";

        public string Notes { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } =
            DateTime.UtcNow;
    }
}