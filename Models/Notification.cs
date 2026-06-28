using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MathUniverse.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? LinkUrl { get; set; }
    }

    public enum NotificationType
    {
        System = 1,
        LearningReminder = 2,
        NewLesson = 3,
        Achievement = 4,
        WeeklyReport = 5
    }
}

