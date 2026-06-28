using MathUniverse.Data;
using MathUniverse.Models;
using Microsoft.EntityFrameworkCore;

namespace MathUniverse.Services
{
    public interface ILessonService
    {
        Task<List<Lesson>> GetLessonsByGradeAsync(int grade);
        Task<Lesson?> GetLessonByIdAsync(int lessonId);
        Task<List<Lesson>> GetAllLessonsAsync();
        Task<Lesson> CreateLessonAsync(Lesson lesson);
        Task<bool> UpdateLessonAsync(Lesson lesson);
        Task<bool> DeleteLessonAsync(int lessonId);
        Task<List<Lesson>> SearchLessonsAsync(string searchTerm, int? grade = null);
    }

    public class LessonService : ILessonService
    {
        private readonly ApplicationDbContext _context;

        public LessonService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Lesson>> GetLessonsByGradeAsync(int grade)
        {
            return await _context.Lessons
                .Where(l => l.Grade == grade && !l.IsDeleted)
                .OrderBy(l => l.OrderIndex)
                .Include(l => l.Exercises)
                .ToListAsync();
        }

        public async Task<Lesson?> GetLessonByIdAsync(int lessonId)
        {
            return await _context.Lessons
                .Include(l => l.Exercises)
                    .ThenInclude(e => e.Questions)
                        .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);
        }

        public async Task<List<Lesson>> GetAllLessonsAsync()
        {
            return await _context.Lessons
                .Where(l => !l.IsDeleted)
                .OrderBy(l => l.Grade)
                .ThenBy(l => l.OrderIndex)
                .Include(l => l.Exercises)
                .ToListAsync();
        }

        public async Task<Lesson> CreateLessonAsync(Lesson lesson)
        {
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            
            // If the lesson is published, automatically create StudentProgress for all students in the same grade
            if (lesson.IsPublished)
            {
                await CreateProgressForAllStudentsAsync(lesson);
            }
            
            return lesson;
        }

        private async Task CreateProgressForAllStudentsAsync(Lesson lesson)
        {
            // Get all students in the same grade
            var students = await _context.Students
                .Where(s => s.Grade == lesson.Grade && s.IsActive)
                .ToListAsync();

            // Determine if this lesson should be unlocked
            // Unlock if: OrderIndex = 1 OR no PreviousLessonId OR it's the first lesson for this grade
            var isFirstLesson = lesson.OrderIndex == 1 || lesson.PreviousLessonId == null;
            
            if (!isFirstLesson)
            {
                // Check if there are any lessons with lower OrderIndex
                var hasEarlierLessons = await _context.Lessons
                    .AnyAsync(l => l.Grade == lesson.Grade && 
                                  l.LessonId != lesson.LessonId && 
                                  l.OrderIndex < lesson.OrderIndex &&
                                  l.IsPublished);
                
                isFirstLesson = !hasEarlierLessons;
            }

            foreach (var student in students)
            {
                // Check if progress already exists
                var existingProgress = await _context.StudentProgress
                    .FirstOrDefaultAsync(sp => sp.StudentId == student.StudentId && 
                                              sp.LessonId == lesson.LessonId);

                if (existingProgress == null)
                {
                    var progress = new StudentProgress
                    {
                        StudentId = student.StudentId,
                        LessonId = lesson.LessonId,
                        VideoTotalSeconds = lesson.VideoDuration,
                        Status = ProgressStatus.NotStarted,
                        IsUnlocked = isFirstLesson // First lesson is unlocked by default
                    };
                    _context.StudentProgress.Add(progress);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateLessonAsync(Lesson lesson)
        {
            lesson.UpdatedDate = DateTime.Now;
            
            // Check if the lesson is being published for the first time
            var existingLesson = await _context.Lessons
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LessonId == lesson.LessonId);
            
            bool wasUnpublished = existingLesson != null && !existingLesson.IsPublished;
            bool isNowPublished = lesson.IsPublished;
            
            _context.Lessons.Update(lesson);
            var result = await _context.SaveChangesAsync() > 0;
            
            // If the lesson is being published now (was unpublished before), create progress for students
            if (result && wasUnpublished && isNowPublished)
            {
                await CreateProgressForAllStudentsAsync(lesson);
            }
            
            return result;
        }

        public async Task<bool> DeleteLessonAsync(int lessonId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Exercises)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);
            
            if (lesson == null) return false;

            // Soft delete - đánh dấu là đã xóa
            lesson.IsDeleted = true;
            lesson.UpdatedDate = DateTime.Now;
            
            // Lấy tất cả ExerciseIds thuộc bài giảng này
            var exerciseIds = lesson.Exercises.Select(e => e.ExerciseId).ToList();
            
            if (exerciseIds.Any())
            {
                // Lấy danh sách học sinh có làm bài tập của bài giảng này
                var affectedStudents = await _context.ExerciseResults
                    .Where(er => exerciseIds.Contains(er.ExerciseId) && er.GradingStatus == GradingStatus.Graded)
                    .Select(er => er.StudentId)
                    .Distinct()
                    .ToListAsync();
                
                // Recalculate TotalPoints cho từng học sinh bị ảnh hưởng
                // Vì bài giảng bị xóa, điểm từ bài này sẽ không được tính nữa
                foreach (var studentId in affectedStudents)
                {
                    // Tính lại điểm từ các bài tập còn lại (bài chưa bị xóa)
                    var remainingScores = await _context.ExerciseResults
                        .Where(er => er.StudentId == studentId && er.GradingStatus == GradingStatus.Graded)
                        .Join(
                            _context.Exercises,
                            er => er.ExerciseId,
                            e => e.ExerciseId,
                            (er, e) => new { er, e }
                        )
                        .Join(
                            _context.Lessons.Where(l => !l.IsDeleted),
                            x => x.e.LessonId,
                            l => l.LessonId,
                            (x, l) => x.er
                        )
                        .GroupBy(er => er.ExerciseId)
                        .Select(g => new
                        {
                            ExerciseId = g.Key,
                            HighestScore = g.Max(er => er.Score)
                        })
                        .ToListAsync();
                    
                    double newTotalPoints = remainingScores.Sum(x => x.HighestScore);
                    
                    var student = await _context.Students.FindAsync(studentId);
                    if (student != null)
                    {
                        student.TotalPoints = (int)Math.Round(newTotalPoints);
                    }
                }
            }
            
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Lesson>> SearchLessonsAsync(string searchTerm, int? grade = null)
        {
            var query = _context.Lessons.Where(l => !l.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(l => l.Title.Contains(searchTerm) || 
                                       l.Topic.Contains(searchTerm) ||
                                       (l.Description != null && l.Description.Contains(searchTerm)));
            }

            if (grade.HasValue)
            {
                query = query.Where(l => l.Grade == grade.Value);
            }

            return await query
                .Where(l => l.IsPublished)
                .OrderBy(l => l.Grade)
                .ThenBy(l => l.OrderIndex)
                .ToListAsync();
        }
    }
}

