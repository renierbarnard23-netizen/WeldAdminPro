// WeldAdminPro.Core/Models/User.cs  (or the file you currently have)
using System;
using System.ComponentModel.DataAnnotations;

namespace WeldAdminPro.Core.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = null!;

        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Role { get; set; }

        // Add these two properties (nullable to match existing DB rows)
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
