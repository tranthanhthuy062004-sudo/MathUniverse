using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MathUniverse.Models
{
    public class Exercise
    {
        [Key]
        public int ExerciseId { get; set; }

        [Required]
        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ExerciseType Type { get; set; }

        public DifficultyLevel Difficulty { get; set; }

        public int MaxAttempts { get; set; } = 3;

        public double PassingScore { get; set; } = 5.0;

        public int TimeLimit { get; set; } = 0;

        public int OrderIndex { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<ExerciseResult> Results { get; set; } = new List<ExerciseResult>();
    }

    public enum ExerciseType
    {
        MultipleChoice = 1,
        DragAndDrop = 2,
        Matching = 3,
        Interactive = 4
    }

    public enum DifficultyLevel
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }
}

