﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MathUniverse.Models
{
    public class ExerciseResult
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student Student { get; set; } = null!;

        [Required]
        public int ExerciseId { get; set; }

        [ForeignKey("ExerciseId")]
        public Exercise Exercise { get; set; } = null!;

        public double Score { get; set; }

        public int CorrectAnswers { get; set; }

        public int TotalQuestions { get; set; }

        public int TimeSpent { get; set; }

        public int AttemptNumber { get; set; }

        public bool IsPassed { get; set; }

        public DateTime CompletedDate { get; set; } = DateTime.Now;

        public string? AnswersJson { get; set; }

        public GradingStatus GradingStatus { get; set; } = GradingStatus.Graded;

        public ICollection<EssayAnswer> EssayAnswers { get; set; } = new List<EssayAnswer>();
    }

    public enum GradingStatus
    {
        Graded = 0,          // Đã chấm xong (không có tự luận hoặc đã chấm hết) - Default
        PendingGrading = 1   // Đang chờ chấm tự luận
    }
}

