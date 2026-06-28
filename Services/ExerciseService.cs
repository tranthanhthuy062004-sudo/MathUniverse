﻿﻿using MathUniverse.Data;
using MathUniverse.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MathUniverse.Services
{
    public interface IExerciseService
    {
        Task<Exercise?> GetExerciseByIdAsync(int exerciseId);
        Task<List<Exercise>> GetExercisesByLessonAsync(int lessonId);
        Task<Exercise> CreateExerciseAsync(Exercise exercise);
        Task<bool> UpdateExerciseAsync(Exercise exercise);
        Task<bool> DeleteExerciseAsync(int exerciseId);
        Task<ExerciseResult> SubmitExerciseAsync(int studentId, int exerciseId, Dictionary<int, int> answers, int timeSpent);
        Task<List<ExerciseResult>> GetStudentResultsAsync(int studentId, int exerciseId);
        Task<bool> CanAttemptExerciseAsync(int studentId, int exerciseId);
        Task RecalculateStudentTotalPointsAsync(int studentId);
    }

    public class ExerciseService : IExerciseService
    {
        private readonly ApplicationDbContext _context;

        public ExerciseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Exercise?> GetExerciseByIdAsync(int exerciseId)
        {
            return await _context.Exercises
                .Include(e => e.Lesson)
                .Include(e => e.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId);
        }

        public async Task<List<Exercise>> GetExercisesByLessonAsync(int lessonId)
        {
            return await _context.Exercises
                .Where(e => e.LessonId == lessonId && e.IsActive)
                .OrderBy(e => e.OrderIndex)
                .Include(e => e.Questions)
                .ToListAsync();
        }

        public async Task<Exercise> CreateExerciseAsync(Exercise exercise)
        {
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            return exercise;
        }

        public async Task<bool> UpdateExerciseAsync(Exercise exercise)
        {
            _context.Exercises.Update(exercise);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteExerciseAsync(int exerciseId)
        {
            var exercise = await _context.Exercises.FindAsync(exerciseId);
            if (exercise == null) return false;

            exercise.IsActive = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ExerciseResult> SubmitExerciseAsync(int studentId, int exerciseId, Dictionary<int, int> answers, int timeSpent)
        {
            var exercise = await GetExerciseByIdAsync(exerciseId);
            if (exercise == null)
                throw new ArgumentException("Exercise not found");

            // Count previous attempts
            var previousAttempts = await _context.ExerciseResults
                .CountAsync(er => er.StudentId == studentId && er.ExerciseId == exerciseId);

            // Check correct answers (only for multiple choice questions)
            int correctCount = 0;
            int totalPoints = 0;
            int earnedPoints = 0;

            var questionResults = new List<object>();

            var multipleChoiceQuestions = exercise.Questions.Where(q => q.Type == QuestionType.MultipleChoice).ToList();
            var essayQuestions = exercise.Questions.Where(q => q.Type == QuestionType.Essay).ToList();

            foreach (var question in multipleChoiceQuestions)
            {
                totalPoints += question.Points;
                
                if (answers.TryGetValue(question.QuestionId, out int selectedAnswerId))
                {
                    var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
                    var isCorrect = correctAnswer?.AnswerId == selectedAnswerId;

                    if (isCorrect)
                    {
                        correctCount++;
                        earnedPoints += question.Points;
                    }

                    questionResults.Add(new
                    {
                        QuestionId = question.QuestionId,
                        SelectedAnswerId = selectedAnswerId,
                        CorrectAnswerId = correctAnswer?.AnswerId,
                        IsCorrect = isCorrect
                    });
                }
            }

            // Calculate score (0-10 scale) - only from multiple choice questions for now
            double score = 0;
            if (multipleChoiceQuestions.Any())
            {
                score = (double)earnedPoints / totalPoints * 10;
            }
            
            // Check if has essay questions - if yes, mark as pending grading
            bool hasEssayQuestions = essayQuestions.Any();
            GradingStatus gradingStatus = hasEssayQuestions ? GradingStatus.PendingGrading : GradingStatus.Graded;
            bool isPassed = !hasEssayQuestions && score >= exercise.PassingScore;

            var result = new ExerciseResult
            {
                StudentId = studentId,
                ExerciseId = exerciseId,
                Score = Math.Round(score, 2),
                CorrectAnswers = correctCount,
                TotalQuestions = multipleChoiceQuestions.Count,
                TimeSpent = timeSpent,
                AttemptNumber = previousAttempts + 1,
                IsPassed = isPassed,
                GradingStatus = gradingStatus,
                CompletedDate = DateTime.Now,
                AnswersJson = JsonSerializer.Serialize(questionResults)
            };

            _context.ExerciseResults.Add(result);
            await _context.SaveChangesAsync();

            // Only recalculate points if not pending grading
            if (gradingStatus == GradingStatus.Graded)
            {
                await RecalculateStudentTotalPointsAsync(studentId);
            }

            return result;
        }

        public async Task<List<ExerciseResult>> GetStudentResultsAsync(int studentId, int exerciseId)
        {
            return await _context.ExerciseResults
                .Where(er => er.StudentId == studentId && er.ExerciseId == exerciseId)
                .OrderByDescending(er => er.CompletedDate)
                .ToListAsync();
        }

        public async Task<bool> CanAttemptExerciseAsync(int studentId, int exerciseId)
        {
            var exercise = await _context.Exercises.FindAsync(exerciseId);
            if (exercise == null) return false;

            var attemptCount = await _context.ExerciseResults
                .CountAsync(er => er.StudentId == studentId && er.ExerciseId == exerciseId);

            return attemptCount < exercise.MaxAttempts;
        }

        public async Task RecalculateStudentTotalPointsAsync(int studentId)
        {
            // Lấy điểm cao nhất của mỗi bài tập (chỉ tính bài đã chấm xong)
            var highestScores = await _context.ExerciseResults
                .Where(er => er.StudentId == studentId && er.GradingStatus == GradingStatus.Graded)
                .GroupBy(er => er.ExerciseId)
                .Select(g => new
                {
                    ExerciseId = g.Key,
                    HighestScore = g.Max(er => er.Score)
                })
                .ToListAsync();

            // Tính tổng điểm: Tổng điểm cao nhất của mỗi bài tập
            double totalPoints = highestScores.Sum(hs => hs.HighestScore);

            // Cập nhật TotalPoints cho học sinh
            var student = await _context.Students.FindAsync(studentId);
            if (student != null)
            {
                student.TotalPoints = (int)Math.Round(totalPoints);
                await _context.SaveChangesAsync();
            }
        }
    }
}

