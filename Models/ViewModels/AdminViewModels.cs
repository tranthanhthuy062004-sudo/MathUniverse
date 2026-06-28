﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MathUniverse.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int TotalLessons { get; set; }
        public int TotalExercises { get; set; }
        public List<StudentPerformanceViewModel> RecentStudentActivity { get; set; } = new();
        public Dictionary<int, int> StudentsByGrade { get; set; } = new();
        public Dictionary<string, double> AverageScoresByTopic { get; set; } = new();
    }

    public class StudentPerformanceViewModel
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Grade { get; set; }
        public int LessonsCompleted { get; set; }
        public double AverageScore { get; set; }
        public DateTime LastActivity { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ManageStudentViewModel
    {
        public List<Student> Students { get; set; } = new();
        public int? FilterGrade { get; set; }
        public string? SearchTerm { get; set; }
        public bool? ShowInactiveOnly { get; set; }
    }

    public class CreateLessonViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài học")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        [Range(1, 5)]
        public int Grade { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chủ đề")]
        [StringLength(100)]
        public string Topic { get; set; } = string.Empty;

        public string? VideoUrl { get; set; }


        public IFormFile? PdfFile { get; set; } // File PDF upload

        public string? ExistingPdfUrl { get; set; } // URL của PDF hiện tại (dùng khi edit)

        public string? TheoryContent { get; set; }

        public string? ThumbnailUrl { get; set; }

        public int OrderIndex { get; set; }

        public bool IsPublished { get; set; }

        public int? PreviousLessonId { get; set; }
    }

    public class CreateExerciseViewModel
    {
        [Required]
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài tập")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ExerciseType Type { get; set; } = ExerciseType.MultipleChoice; // Mặc định là trắc nghiệm

        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy; // Mặc định là dễ

        [Range(1, 10)]
        public int MaxAttempts { get; set; } = 3;

        [Range(0, 10)]
        public double PassingScore { get; set; } = 5.0;

        public int TimeLimit { get; set; } = 0;

        public int OrderIndex { get; set; }

        public List<CreateQuestionViewModel> Questions { get; set; } = new();
    }

    public class CreateQuestionViewModel
    {
        public QuestionType Type { get; set; } = QuestionType.MultipleChoice; // Loại câu hỏi

        [Required(ErrorMessage = "Vui lòng nhập câu hỏi")]
        public string QuestionText { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? AudioUrl { get; set; }

        [Range(1, 10)]
        public int Points { get; set; } = 1;

        public int OrderIndex { get; set; }

        public List<CreateAnswerViewModel> Answers { get; set; } = new();
    }

    public class CreateAnswerViewModel
    {
        public string? AnswerText { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsCorrect { get; set; }

        public int OrderIndex { get; set; }

        public string? Explanation { get; set; }
    }

    public class StudentDetailViewModel
    {
        public Student Student { get; set; } = null!;
        public StudentStatistics Statistics { get; set; } = new();
        public List<LessonProgressViewModel> LessonProgress { get; set; } = new();
        public List<ExerciseResult> RecentResults { get; set; } = new();
        public List<ActivityLog> RecentActivities { get; set; } = new();
    }
}

