using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MathUniverse.Models
{
    public class StudentProgress
    {
        [Key]
        public int ProgressId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;

        [Required]
        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; } = null!;

        public ProgressStatus Status { get; set; }

        public int VideoWatchedSeconds { get; set; } = 0;

        public int VideoTotalSeconds { get; set; } = 0;

        public double CompletionPercentage { get; set; } = 0;

        public DateTime? StartedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime LastAccessedDate { get; set; } = DateTime.Now;

        public bool IsUnlocked { get; set; } = false;
    }

    public enum ProgressStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Passed = 3
    }
}

