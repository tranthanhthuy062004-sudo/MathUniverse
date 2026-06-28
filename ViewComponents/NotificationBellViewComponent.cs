using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MathUniverse.Data;
using MathUniverse.Models;
using MathUniverse.Services;

namespace MathUniverse.ViewComponents
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public NotificationBellViewComponent(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                // Check if user is authenticated
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return Content(string.Empty);
                }

                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    return Content(string.Empty);
                }

                // Don't show for Admin
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (isAdmin)
                {
                    return Content(string.Empty);
                }

                // Try to get student profile
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

                // If student profile is missing, still show the bell with 0 notifications
                if (student == null)
                {
                    ViewBag.UnreadCount = 0;
                    return View(new List<Notification>());
                }

                var notifications = await _notificationService.GetStudentNotificationsAsync(student.StudentId, unreadOnly: false);
                var unreadCount = await _notificationService.GetUnreadCountAsync(student.StudentId);

                ViewBag.UnreadCount = unreadCount;
                return View(notifications.Take(10).ToList());
            }
            catch
            {
                // If anything goes wrong, just don't show the notification bell
                return Content(string.Empty);
            }
        }
    }
}
