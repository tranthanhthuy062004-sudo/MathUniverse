using MathUniverse.Data;
using MathUniverse.Models;
using Microsoft.EntityFrameworkCore;

namespace MathUniverse.Services
{
    public interface INotificationService
    {
        Task<List<Notification>> GetStudentNotificationsAsync(int studentId, bool unreadOnly = false);
        Task<Notification> CreateNotificationAsync(int studentId, string title, string message, NotificationType type, string? linkUrl = null);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int studentId);
        Task<int> GetUnreadCountAsync(int studentId);
        Task SendLearningReminderAsync(int studentId);
        Task SendWeeklyReportAsync(int studentId);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetStudentNotificationsAsync(int studentId, bool unreadOnly = false)
        {
            var query = _context.Notifications.Where(n => n.StudentId == studentId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedDate)
                .Take(50)
                .ToListAsync();
        }

        public async Task<Notification> CreateNotificationAsync(int studentId, string title, string message, NotificationType type, string? linkUrl = null)
        {
            var notification = new Notification
            {
                StudentId = studentId,
                Title = title,
                Message = message,
                Type = type,
                LinkUrl = linkUrl,
                IsRead = false,
                CreatedDate = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkAllAsReadAsync(int studentId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.StudentId == studentId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> GetUnreadCountAsync(int studentId)
        {
            return await _context.Notifications
                .CountAsync(n => n.StudentId == studentId && !n.IsRead);
        }

        public async Task SendLearningReminderAsync(int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return;

            await CreateNotificationAsync(
                studentId,
                "Nhắc nhở học tập",
                $"Chào {student.FullName}! Đã lâu rồi bạn chưa học bài. Hãy tiếp tục hành trình khám phá toán học nhé! 📚",
                NotificationType.LearningReminder,
                "/Student/Dashboard"
            );
        }

        public async Task SendWeeklyReportAsync(int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return;

            var weekAgo = DateTime.Now.AddDays(-7);
            var completedLessons = await _context.StudentProgress
                .CountAsync(sp => sp.StudentId == studentId && 
                                 sp.Status == ProgressStatus.Passed &&
                                 sp.CompletedDate >= weekAgo);

            var averageScore = await _context.ExerciseResults
                .Where(er => er.StudentId == studentId && er.CompletedDate >= weekAgo)
                .AverageAsync(er => (double?)er.Score) ?? 0;

            await CreateNotificationAsync(
                studentId,
                "Báo cáo tuần học tập",
                $"Tuần này bạn đã hoàn thành {completedLessons} bài học với điểm trung bình {averageScore:F1}. Tiếp tục phát huy nhé! 🌟",
                NotificationType.WeeklyReport,
                "/Student/Progress"
            );
        }
    }
}

