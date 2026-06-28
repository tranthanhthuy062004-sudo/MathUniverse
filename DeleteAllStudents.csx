using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MathUniverse.Data;
using MathUniverse.Models;

// Script để xóa tất cả student accounts trực tiếp từ database
// Chạy: dotnet script DeleteAllStudents.csx

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    Console.WriteLine("Starting deletion of all student accounts...");

    // Get all users
    var allUsers = await userManager.Users.ToListAsync();
    var studentsToDelete = new List<ApplicationUser>();

    foreach (var user in allUsers)
    {
        var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
        if (!isAdmin)
        {
            studentsToDelete.Add(user);
        }
    }

    Console.WriteLine($"Found {studentsToDelete.Count} student accounts to delete.");

    foreach (var student in studentsToDelete)
    {
        Console.WriteLine($"Deleting student: {student.Email} (ID: {student.Id})");

        // Delete associated Student profile
        var studentProfile = await context.Students
            .FirstOrDefaultAsync(s => s.UserId == student.Id);

        if (studentProfile != null)
        {
            // Delete ExerciseResults first (foreign key constraint)
            var results = context.ExerciseResults.Where(r => r.StudentId == studentProfile.StudentId);
            context.ExerciseResults.RemoveRange(results);
            await context.SaveChangesAsync();

            // Delete StudentProgress
            var progress = context.StudentProgress.Where(p => p.StudentId == studentProfile.StudentId);
            context.StudentProgress.RemoveRange(progress);
            await context.SaveChangesAsync();

            // Delete Notifications
            var notifications = context.Notifications.Where(n => n.StudentId == studentProfile.StudentId);
            context.Notifications.RemoveRange(notifications);
            await context.SaveChangesAsync();

            // Delete Student profile
            context.Students.Remove(studentProfile);
            await context.SaveChangesAsync();
        }

        // Delete activity logs
        string userId = student.Id.ToString();
        var activityLogs = context.ActivityLogs.Where(a => a.UserId == userId);
        context.ActivityLogs.RemoveRange(activityLogs);
        await context.SaveChangesAsync();

        // Delete user account
        var result = await userManager.DeleteAsync(student);
        if (result.Succeeded)
        {
            Console.WriteLine($"  ✓ Successfully deleted {student.Email}");
        }
        else
        {
            Console.WriteLine($"  ✗ Failed to delete {student.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    Console.WriteLine("All student accounts deleted successfully!");
}

