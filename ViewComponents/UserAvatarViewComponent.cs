using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MathUniverse.Data;
using MathUniverse.Models;

namespace MathUniverse.ViewComponents
{
    public class UserAvatarViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserAvatarViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return Content("");
            }

            var userName = user.FullName ?? user.UserName ?? "User";
            string avatarUrl;

            // Admin không có Student profile, dùng icon mặc định
            if (user.Role == UserRole.Admin)
            {
                avatarUrl = "https://cdn-icons-png.flaticon.com/512/4205/4205906.png"; // Admin/teacher icon
            }
            else
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);
                avatarUrl = student?.AvatarUrl ?? "https://cdn-icons-png.flaticon.com/512/2922/2922510.png";
            }

            return View(new UserAvatarViewModel
            {
                AvatarUrl = avatarUrl,
                UserName = userName
            });
        }
    }

    public class UserAvatarViewModel
    {
        public string AvatarUrl { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}

