﻿namespace MathUniverse.Models.ViewModels
{
    public class StudentDashboardViewModel
    {
        public Student Student { get; set; } = null!;
        public List<LessonViewModel> RecentLessons { get; set; } = new();
        public List<Notification> RecentNotifications { get; set; } = new();
        public StudentStatistics Statistics { get; set; } = new();
        public List<RankingViewModel> TopStudents { get; set; } = new();
    }

    public class StudentStatistics
    {
        public int TotalLessonsCompleted { get; set; }
        public int TotalExercisesPassed { get; set; }
        public double AverageScore { get; set; }
        public int TotalPoints { get; set; }
        public int BadgesEarned { get; set; }
        public int CurrentRank { get; set; }
        public int DaysStreak { get; set; }
        public Dictionary<string, int> TopicProgress { get; set; } = new();
    }

    public class RankingViewModel
    {
        public int Rank { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Grade { get; set; }
        public int TotalPoints { get; set; }
        public int LessonsCompleted { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    public class ProgressReportViewModel
    {
        public Student Student { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<LessonProgressViewModel> LessonProgress { get; set; } = new();
        public List<ExerciseResult> RecentResults { get; set; } = new();
        public StudentStatistics Statistics { get; set; } = new();
    }

    public class LessonProgressViewModel
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public ProgressStatus Status { get; set; }
        public double CompletionPercentage { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int ExercisesPassed { get; set; }
        public int TotalExercises { get; set; }
    }

    public class StudentProfileViewModel
    {
        public int StudentId { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Mã học sinh")]
        public string StudentCode { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng chọn lớp")]
        [System.ComponentModel.DataAnnotations.Range(1, 5, ErrorMessage = "Lớp học phải từ 1 đến 5")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Lớp")]
        public int Grade { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Ngày sinh")]
        public DateTime DateOfBirth { get; set; }

        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Email phụ huynh")]
        public string? ParentEmail { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Ảnh đại diện")]
        public string? AvatarUrl { get; set; }

        public int TotalPoints { get; set; }
        public int BadgesEarned { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
