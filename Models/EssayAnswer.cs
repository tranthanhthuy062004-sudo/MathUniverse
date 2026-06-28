using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MathUniverse.Models
{
    public class EssayAnswer
    {
        [Key]
        public int EssayAnswerId { get; set; }

        [Required]
        public int ExerciseResultId { get; set; }

        [ForeignKey("ExerciseResultId")]
        public ExerciseResult ExerciseResult { get; set; } = null!;

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public Question Question { get; set; } = null!;

        [Required]
        public string AnswerText { get; set; } = string.Empty; // Câu trả lời của học sinh

        public string? ImageUrl { get; set; } // Ảnh câu trả lời (nếu có)

        public double? Score { get; set; } // Điểm do admin chấm (null = chưa chấm)

        public string? Feedback { get; set; } // Phản hồi từ admin

        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        public DateTime? GradedDate { get; set; } // Ngày chấm điểm

        public int? GradedByAdminId { get; set; } // Admin nào chấm
    }
}

