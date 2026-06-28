using Microsoft.AspNetCore.Identity;
using MathUniverse.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MathUniverse.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            context.Database.EnsureCreated();

            // Seed Roles
            string[] roleNames = { "Admin", "Student", "Guest" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin User
            var adminEmail = "admin@mathuniverse.com";
            
            // Check if admin already exists using direct query to avoid SingleOrDefault issues
            var existingAdmin = userManager.Users.FirstOrDefault(u => u.Email == adminEmail);

            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Giáo viên",
                    Role = UserRole.Admin,
                    CreatedDate = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    // Create Admin profile
                    var adminProfile = new Admin
                    {
                        UserId = adminUser.Id,
                        FullName = "Administrator",
                        Position = "System Admin",
                        JoinDate = DateTime.Now,
                        IsActive = true
                    };

                    context.Admins.Add(adminProfile);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}

