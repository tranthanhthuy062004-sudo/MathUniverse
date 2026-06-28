using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MathUniverse.Models;
using MathUniverse.Models.ViewModels;
using MathUniverse.Data;

namespace MathUniverse.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var email = model.UserName?.Trim();

                // Tìm user theo Email (không phân biệt hoa thường)
                ApplicationUser? user = null;
                if (!string.IsNullOrEmpty(email))
                {
                    // Sử dụng FirstOrDefault thay vì FindByEmailAsync để tránh lỗi khi có duplicate emails
                    var normalizedEmail = email.ToUpper();
                    user = _userManager.Users.FirstOrDefault(u => u.NormalizedEmail == normalizedEmail);
                    
                    // Fallback: Tìm trực tiếp trong database nếu không tìm thấy qua NormalizedEmail
                    if (user == null)
                    {
                        user = _userManager.Users.FirstOrDefault(u => u.Email != null && u.Email.ToLower() == email.ToLower());
                    }
                }

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email không tồn tại trong hệ thống.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    user.LastLoginDate = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    if (user.Role == UserRole.Admin)
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (user.Role == UserRole.Student)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    return LocalRedirect(returnUrl ?? "/");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng thử lại sau.");
                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản chưa được xác thực hoặc không được phép đăng nhập.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu không chính xác. Vui lòng thử lại.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var year = DateTime.Now.Year % 100;
                var dayOfYear = DateTime.Now.DayOfYear;
                var count = _context.Students.Count() + 1;
                var studentCode = $"{year:D2}{dayOfYear:D3}{count:D3}";

                var user = new ApplicationUser
                {
                    UserName = studentCode,
                    FullName = model.FullName,
                    Role = UserRole.Student,
                    CreatedDate = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Set email và confirm ngay lập tức
                    if (!string.IsNullOrEmpty(model.ParentEmail))
                    {
                        var email = model.ParentEmail.Trim();
                        await _userManager.SetEmailAsync(user, email);
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        await _userManager.ConfirmEmailAsync(user, token);
                    }

                    var student = new Student
                    {
                        UserId = user.Id,
                        StudentCode = studentCode,
                        FullName = model.FullName,
                        Grade = model.Grade,
                        DateOfBirth = model.DateOfBirth,
                        ParentEmail = model.ParentEmail, // Lưu email gốc vào Student
                        EnrollmentDate = DateTime.Now,
                        IsActive = true
                    };

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    var progressService = new Services.StudentProgressService(_context);
                    await progressService.InitializeProgressForStudentAsync(student.StudentId, student.Grade);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    TempData["SuccessMessage"] = $"Đăng ký thành công! Mã học sinh của bạn là: {studentCode}";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

