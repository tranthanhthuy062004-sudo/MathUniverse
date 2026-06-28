using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MathUniverse.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required]
        [StringLength(20)]
        public string StudentCode { get; set; } = string.Empty; // Mã học sinh (VD: 23010xxx)

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int Grade { get; set; } // Lớp (1-5)

        public DateTime DateOfBirth { get; set; }

        [StringLength(200)]
        public string? AvatarUrl { get; set; }

        [EmailAddress]
        public string? ParentEmail { get; set; } // Email phụ huynh

        public int TotalPoints { get; set; } = 0;

        public int BadgesEarned { get; set; } = 0;

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<StudentProgress> Progress { get; set; } = new List<StudentProgress>();
        public ICollection<ExerciseResult> ExerciseResults { get; set; } = new List<ExerciseResult>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}

