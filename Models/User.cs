using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MathUniverse.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FullName { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? LastLoginDate { get; set; }
        
        public UserRole Role { get; set; }
        
        // Navigation properties
        public Student? Student { get; set; }
        public Admin? Admin { get; set; }
    }

    public enum UserRole
    {
        Guest = 0,
        Student = 1,
        Admin = 2,
        SystemHandler = 3
    }
}

