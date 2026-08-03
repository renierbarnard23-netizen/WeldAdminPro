using System;

namespace WeldAdminPro.Core.Models
{
    public class SystemUser
    {
        public Guid Id { get; set; } =
            Guid.NewGuid();

        public string Username { get; set; } = "";

        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";

        public string PasswordHash { get; set; } = "";

        // ============================================
        // DATABASE ROLE ARCHITECTURE
        // ============================================

        public int RoleId { get; set; }

        public string RoleName { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }
    }
}