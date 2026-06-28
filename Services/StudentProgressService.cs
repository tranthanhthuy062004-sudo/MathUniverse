﻿﻿using MathUniverse.Data;
using MathUniverse.Models;
using Microsoft.EntityFrameworkCore;

namespace MathUniverse.Services
{
    public interface IStudentProgressService
    {
        Task<StudentProgress?> GetProgressAsync(int studentId, int lessonId);
        Task<List<StudentProgress>> GetStudentProgressAsync(int studentId);
        Task<bool> UpdateVideoProgressAsync(int studentId, int lessonId, int watchedSeconds);
        Task<bool> MarkLessonCompletedAsync(int studentId, int lessonId);
        Task<bool> UnlockNextLessonAsync(int studentId, int currentLessonId);
        Task<bool> CheckAndUnlockNextLessonAsync(int studentId, int lessonId);
        Task CheckAndUnlockAllEligibleLessonsAsync(int studentId, int grade);
        Task InitializeProgressForStudentAsync(int studentId, int grade);
        Task<StudentProgress> AutoCompletePdfLessonAsync(int studentId, int lessonId);
    }

    public class StudentProgressService : IStudentProgressService
    {
        private readonly ApplicationDbContext _context;

        public StudentProgressService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentProgress?> GetProgressAsync(int studentId, int lessonId)
        {
            return await _context.StudentProgress
                .Include(sp => sp.Lesson)
                .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.LessonId == lessonId);
        }

        public async Task<List<StudentProgress>> GetStudentProgressAsync(int studentId)
        {
            return await _context.StudentProgress
                .Include(sp => sp.Lesson)
                    .ThenInclude(l => l.Exercises)
                .Where(sp => sp.StudentId == studentId)
                .OrderBy(sp => sp.Lesson.Grade)
                .ThenBy(sp => sp.Lesson.OrderIndex)
                .ToListAsync();
        }

        public async Task<bool> UpdateVideoProgressAsync(int studentId, int lessonId, int watchedSeconds)
        {
            var progress = await GetProgressAsync(studentId, lessonId);
            
            if (progress == null)
            {
                var lesson = await _context.Lessons.FindAsync(lessonId);
                if (lesson == null) return false;

                progress = new StudentProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    VideoTotalSeconds = lesson.VideoDuration,
                    VideoWatchedSeconds = watchedSeconds,
                    Status = ProgressStatus.InProgress,
                    StartedDate = DateTime.Now,
                    IsUnlocked = true
                };
                _context.StudentProgress.Add(progress);
            }
            else
            {
                progress.VideoWatchedSeconds = Math.Max(progress.VideoWatchedSeconds, watchedSeconds);
                progress.Status = ProgressStatus.InProgress;
            }

            progress.CompletionPercentage = progress.VideoTotalSeconds > 0 
                ? (double)progress.VideoWatchedSeconds / progress.VideoTotalSeconds * 100 
                : 0;
            progress.LastAccessedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            // Tự động kiểm tra điều kiện mở khóa bài tiếp khi xem đủ video (>= 90%)
            if (progress.CompletionPercentage >= 90)
            {
                await CheckAndUnlockNextLessonAsync(studentId, lessonId);
            }

            return true;
        }

        public async Task<bool> MarkLessonCompletedAsync(int studentId, int lessonId)
        {
            var progress = await GetProgressAsync(studentId, lessonId);
            if (progress == null) return false;

            // Kiểm tra điều kiện mở khóa bài tiếp theo:
            // 1. Đã xem video (CompletionPercentage >= 90%)
            // 2. Đã làm ít nhất 1 bài tập đạt >= 70%
            
            var lesson = await _context.Lessons
                .Include(l => l.Exercises)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null) return false;

            // Kiểm tra xem đã có bài tập nào đạt >= 70% chưa
            var exerciseIds = lesson.Exercises.Select(e => e.ExerciseId).ToList();
            var hasPassedExercise = await _context.ExerciseResults
                .AnyAsync(er => er.StudentId == studentId && 
                               exerciseIds.Contains(er.ExerciseId) && 
                               er.IsPassed); // IsPassed = true nghĩa là đạt >= PassingScore (70%)

            // Kiểm tra xem đã xem đủ video chưa (>= 90%)
            bool hasWatchedVideo = progress.CompletionPercentage >= 90;

            // Nếu đã xem video VÀ đã làm ít nhất 1 bài tập đạt
            if (hasWatchedVideo && hasPassedExercise)
            {
                progress.Status = ProgressStatus.Passed;
                progress.CompletedDate = DateTime.Now;
                progress.CompletionPercentage = 100;
                
                await _context.SaveChangesAsync();
                
                // Mở khóa bài học tiếp theo
                await UnlockNextLessonAsync(studentId, lessonId);
                
                return true;
            }

            return false;
        }

        public async Task<bool> UnlockNextLessonAsync(int studentId, int currentLessonId)
        {
            var currentLesson = await _context.Lessons.FindAsync(currentLessonId);
            if (currentLesson == null) return false;

            // Find next lesson
            var nextLesson = await _context.Lessons
                .Where(l => l.Grade == currentLesson.Grade && 
                          l.OrderIndex > currentLesson.OrderIndex &&
                          l.IsPublished)
                .OrderBy(l => l.OrderIndex)
                .FirstOrDefaultAsync();

            if (nextLesson == null) return false;

            // Check if progress exists
            var progress = await GetProgressAsync(studentId, nextLesson.LessonId);
            if (progress == null)
            {
                progress = new StudentProgress
                {
                    StudentId = studentId,
                    LessonId = nextLesson.LessonId,
                    VideoTotalSeconds = nextLesson.VideoDuration,
                    Status = ProgressStatus.NotStarted,
                    IsUnlocked = true
                };
                _context.StudentProgress.Add(progress);
            }
            else
            {
                progress.IsUnlocked = true;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CheckAndUnlockNextLessonAsync(int studentId, int lessonId)
        {
            // Kiểm tra xem bài học hiện tại đã đáp ứng điều kiện mở khóa bài tiếp chưa
            var progress = await GetProgressAsync(studentId, lessonId);
            if (progress == null) return false;

            // Điều kiện 1: Đã xem video >= 90%
            bool hasWatchedVideo = progress.CompletionPercentage >= 90;

            // Điều kiện 2: Đã làm ít nhất 1 bài tập đạt >= 70%
            var lesson = await _context.Lessons
                .Include(l => l.Exercises)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null) return false;

            var exerciseIds = lesson.Exercises.Select(e => e.ExerciseId).ToList();
            bool hasPassedExercise = false;

            if (exerciseIds.Any())
            {
                hasPassedExercise = await _context.ExerciseResults
                    .AnyAsync(er => er.StudentId == studentId &&
                                   exerciseIds.Contains(er.ExerciseId) &&
                                   er.IsPassed);
            }

            // Nếu cả 2 điều kiện đều thỏa mãn → Mở khóa bài tiếp
            if (hasWatchedVideo && hasPassedExercise)
            {
                // Cập nhật trạng thái bài hiện tại
                progress.Status = ProgressStatus.Passed;
                if (progress.CompletedDate == null)
                {
                    progress.CompletedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();

                // Mở khóa bài tiếp theo
                return await UnlockNextLessonAsync(studentId, lessonId);
            }

            return false;
        }

        public async Task InitializeProgressForStudentAsync(int studentId, int grade)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return;

            var lessons = await _context.Lessons
                .Where(l => l.Grade == grade && l.IsPublished)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();

            foreach (var lesson in lessons)
            {
                var existingProgress = await GetProgressAsync(studentId, lesson.LessonId);
                if (existingProgress == null)
                {
                    var progress = new StudentProgress
                    {
                        StudentId = studentId,
                        LessonId = lesson.LessonId,
                        VideoTotalSeconds = lesson.VideoDuration,
                        Status = ProgressStatus.NotStarted,
                        IsUnlocked = lesson.OrderIndex == 1 || lesson.PreviousLessonId == null
                    };
                    _context.StudentProgress.Add(progress);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task CheckAndUnlockAllEligibleLessonsAsync(int studentId, int grade)
        {
            // Lấy tất cả bài học của lớp, sắp xếp theo thứ tự
            var lessons = await _context.Lessons
                .Where(l => l.Grade == grade && l.IsPublished)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();

            // Lấy tất cả progress của học sinh
            var progressList = await _context.StudentProgress
                .Where(sp => sp.StudentId == studentId)
                .Include(sp => sp.Lesson)
                .ToListAsync();

            var progressDict = progressList.ToDictionary(p => p.LessonId);

            // Lấy tất cả kết quả bài tập của học sinh
            var passedExercises = await _context.ExerciseResults
                .Where(er => er.StudentId == studentId && er.IsPassed)
                .Select(er => er.Exercise.LessonId)
                .Distinct()
                .ToListAsync();

            bool hasChanges = false;

            for (int i = 0; i < lessons.Count; i++)
            {
                var lesson = lessons[i];
                
                // Nếu chưa có progress cho bài này, tạo mới
                if (!progressDict.ContainsKey(lesson.LessonId))
                {
                    var newProgress = new StudentProgress
                    {
                        StudentId = studentId,
                        LessonId = lesson.LessonId,
                        VideoTotalSeconds = lesson.VideoDuration,
                        Status = ProgressStatus.NotStarted,
                        IsUnlocked = i == 0 // Bài đầu tiên luôn mở khóa
                    };
                    _context.StudentProgress.Add(newProgress);
                    progressDict[lesson.LessonId] = newProgress;
                    hasChanges = true;
                    continue;
                }
                
                // Bài đầu tiên luôn mở khóa
                if (i == 0)
                {
                    if (!progressDict[lesson.LessonId].IsUnlocked)
                    {
                        progressDict[lesson.LessonId].IsUnlocked = true;
                        hasChanges = true;
                    }
                    continue;
                }

                // Bài tiếp theo: Kiểm tra điều kiện bài trước
                var previousLesson = lessons[i - 1];
                
                if (progressDict.ContainsKey(previousLesson.LessonId))
                {
                    var prevProgress = progressDict[previousLesson.LessonId];
                    
                    // Điều kiện: Đã xem video >= 90% HOẶC đã làm bài tập đạt
                    bool videoCompleted = prevProgress.CompletionPercentage >= 90;
                    bool exercisePassed = passedExercises.Contains(previousLesson.LessonId);
                    
                    // Nếu đáp ứng ít nhất 1 trong 2 điều kiện → Mở khóa bài tiếp
                    if (videoCompleted || exercisePassed)
                    {
                        if (!progressDict[lesson.LessonId].IsUnlocked)
                        {
                            progressDict[lesson.LessonId].IsUnlocked = true;
                            hasChanges = true;
                        }
                    }
                }
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<StudentProgress> AutoCompletePdfLessonAsync(int studentId, int lessonId)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null)
                throw new ArgumentException("Lesson not found");

            // Check if lesson only has PDF (no video)
            if (!string.IsNullOrEmpty(lesson.VideoUrl))
                throw new InvalidOperationException("This method is only for PDF-only lessons");

            var progress = await GetProgressAsync(studentId, lessonId);
            
            if (progress == null)
            {
                // Create new progress with 100% completion
                progress = new StudentProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    VideoTotalSeconds = 0,
                    VideoWatchedSeconds = 0,
                    Status = ProgressStatus.InProgress,
                    CompletionPercentage = 100,
                    StartedDate = DateTime.Now,
                    LastAccessedDate = DateTime.Now,
                    IsUnlocked = true
                };
                _context.StudentProgress.Add(progress);
            }
            else if (progress.CompletionPercentage < 100)
            {
                // Update existing progress to 100%
                progress.CompletionPercentage = 100;
                progress.Status = ProgressStatus.InProgress;
                progress.LastAccessedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // Auto-unlock next lesson for PDF-only lessons
            await UnlockNextLessonAsync(studentId, lessonId);

            return progress;
        }
    }
}


