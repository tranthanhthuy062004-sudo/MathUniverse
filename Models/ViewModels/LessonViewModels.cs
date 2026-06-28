﻿﻿namespace MathUniverse.Models.ViewModels
{
    public class LessonViewModel
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Grade { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public int VideoDuration { get; set; }
        public string? TheoryContent { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int OrderIndex { get; set; }
        public bool IsUnlocked { get; set; }
        public double? CompletionPercentage { get; set; }
        public ProgressStatus? Status { get; set; }
        public int ExerciseCount { get; set; }
    }

    public class LessonDetailViewModel
    {
        public Lesson Lesson { get; set; } = null!;
        public StudentProgress? Progress { get; set; }
        public List<ExerciseViewModel> Exercises { get; set; } = new();
        public bool CanAccessExercises { get; set; }
        public int? NextLessonId { get; set; }
    }

    public class ExerciseViewModel
    {
        public int ExerciseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ExerciseType Type { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public int MaxAttempts { get; set; }
        public double PassingScore { get; set; }
        public int TimeLimit { get; set; }
        public int QuestionCount { get; set; }
        public int? AttemptsUsed { get; set; }
        public double? BestScore { get; set; }
        public bool IsPassed { get; set; }
    }

    public class ExerciseDetailViewModel
    {
        public Exercise Exercise { get; set; } = null!;
        public List<Question> Questions { get; set; } = new();
        public int AttemptsUsed { get; set; }
        public int AttemptsRemaining { get; set; }
        public double? BestScore { get; set; }
        public List<ExerciseResult> PreviousResults { get; set; } = new();
    }

    public class SubmitExerciseViewModel
    {
        public int ExerciseId { get; set; }
        public Dictionary<int, int> Answers { get; set; } = new(); // QuestionId -> AnswerId (for multiple choice)
        public List<EssaySubmissionDto> EssayAnswers { get; set; } = new(); // For essay questions
        public int TimeSpent { get; set; }
    }

    public class EssaySubmissionDto
    {
        public int QuestionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class ExerciseResultViewModel
    {
        public double Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsPassed { get; set; }
        public int AttemptNumber { get; set; }
        public int AttemptsRemaining { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<QuestionResultViewModel> QuestionResults { get; set; } = new();
    }

    public class QuestionResultViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int SelectedAnswerId { get; set; }
        public int CorrectAnswerId { get; set; }
        public bool IsCorrect { get; set; }
        public string? Explanation { get; set; }
    }

    public class CompleteVideoRequest
    {
        public int LessonId { get; set; }
    }

    public class PracticeViewModel
    {
        public int LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public int Grade { get; set; }
        public List<PracticeQuestion> Questions { get; set; } = new();
        public int? PreviousBestScore { get; set; }
        public bool HasPassed { get; set; } // >= 70%
    }

    public class PracticeQuestion
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<PracticeAnswer> Answers { get; set; } = new();
        public int OrderIndex { get; set; }
    }

    public class PracticeAnswer
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class SubmitPracticeViewModel
    {
        public int LessonId { get; set; }
        public Dictionary<int, int> Answers { get; set; } = new(); // QuestionId -> AnswerId
        public int TimeSpent { get; set; }
    }

    public class PracticeResultViewModel
    {
        public int Score { get; set; } // Điểm từ 0-100
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsPassed { get; set; } // >= 70%
        public string Message { get; set; } = string.Empty;
        public List<PracticeQuestionResult> QuestionResults { get; set; } = new();
    }

    public class PracticeQuestionResult
    {
        public string QuestionText { get; set; } = string.Empty;
        public string SelectedAnswer { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string? Explanation { get; set; }
    }
}

