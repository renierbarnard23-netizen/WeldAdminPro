using System;
using System.ComponentModel.DataAnnotations;

namespace WeldAdminPro.Core.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();   // ✅ GUID PK

        [Required]
        public string Username { get; set; } = string.Empty;

        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }

        public string? Role { get; set; }

        // Nullable to match existing / seeded rows
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
