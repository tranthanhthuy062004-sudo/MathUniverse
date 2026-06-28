﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MathUniverse.Models
{
    public enum QuestionType
    {
        MultipleChoice = 1,  // Trắc nghiệm
        Essay = 2            // Tự luận
    }

    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public int ExerciseId { get; set; }

        [ForeignKey("ExerciseId")]
        public Exercise Exercise { get; set; } = null!;

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        public QuestionType Type { get; set; } = QuestionType.MultipleChoice; // Loại câu hỏi

        public string? ImageUrl { get; set; }

        public string? AudioUrl { get; set; }

        public int Points { get; set; } = 1;

        public int OrderIndex { get; set; }

        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }

    public class Answer
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public Question Question { get; set; } = null!;

        [Required]
        public string AnswerText { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public bool IsCorrect { get; set; }

        public int OrderIndex { get; set; }

        public string? Explanation { get; set; }
    }
}

