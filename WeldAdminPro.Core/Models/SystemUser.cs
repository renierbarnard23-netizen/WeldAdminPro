using System;
using WeldAdminPro.Core.Enums;

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

        public SystemRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }
    }
}