﻿using Microsoft.AspNetCore.Identity;
using MathUniverse.Models;
using MathUniverse.Data;
using Microsoft.EntityFrameworkCore;

namespace MathUniverse.Utilities
{
    /// <summary>
    /// Helper class to clean up duplicate admin accounts and manage database cleanup
    /// Call this from Program.cs after app initialization if needed
    /// </summary>
    public static class DatabaseCleanup
    {
        public static async Task DeleteAllStudentAccounts(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            Console.WriteLine("Starting deletion of all student accounts...");
            
            // Get all users who are NOT admins
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
                
                try
                {
                    // Delete associated Student profile
                    var studentProfile = await context.Students
                        .FirstOrDefaultAsync(s => s.UserId == student.Id);
                    
                    if (studentProfile != null)
                    {
                        // Delete ExerciseResults first (foreign key constraint)
                        var results = context.ExerciseResults.Where(r => r.StudentId == studentProfile.StudentId).ToList();
                        if (results.Any())
                        {
                            Console.WriteLine($"  - Deleting {results.Count} exercise results...");
                            context.ExerciseResults.RemoveRange(results);
                            await context.SaveChangesAsync();
                        }
                        
                        // Delete StudentProgress
                        var progress = context.StudentProgress.Where(p => p.StudentId == studentProfile.StudentId).ToList();
                        if (progress.Any())
                        {
                            Console.WriteLine($"  - Deleting {progress.Count} progress records...");
                            context.StudentProgress.RemoveRange(progress);
                            await context.SaveChangesAsync();
                        }
                        
                        // Delete Notifications
                        var notifications = context.Notifications.Where(n => n.StudentId == studentProfile.StudentId).ToList();
                        if (notifications.Any())
                        {
                            Console.WriteLine($"  - Deleting {notifications.Count} notifications...");
                            context.Notifications.RemoveRange(notifications);
                            await context.SaveChangesAsync();
                        }
                        
                        // Delete Student profile
                        Console.WriteLine($"  - Deleting student profile...");
                        context.Students.Remove(studentProfile);
                        await context.SaveChangesAsync();
                    }
                    
                    // Delete activity logs for this user
                    string userId = student.Id.ToString();
                    var activityLogs = context.ActivityLogs.Where(a => a.UserId == userId).ToList();
                    if (activityLogs.Any())
                    {
                        Console.WriteLine($"  - Deleting {activityLogs.Count} activity logs...");
                        context.ActivityLogs.RemoveRange(activityLogs);
                        await context.SaveChangesAsync();
                    }
                    
                    // Delete user account
                    Console.WriteLine($"  - Deleting user account...");
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
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Error deleting {student.Email}: {ex.Message}");
                }
            }
            
            Console.WriteLine("All student accounts deletion process completed!");
        }

        public static async Task CleanupDuplicateAdmins(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            var adminEmail = "admin@mathuniverse.com";
            
            // Find all users with admin email
            var adminUsers = userManager.Users
                .Where(u => u.Email == adminEmail)
                .OrderBy(u => u.CreatedDate)
                .ToList();
            
            if (adminUsers.Count <= 1)
            {
                Console.WriteLine("No duplicate admin accounts found.");
                return;
            }
            
            Console.WriteLine($"Found {adminUsers.Count} admin accounts with email {adminEmail}");
            
            // Keep the first (oldest) admin
            var keepAdmin = adminUsers.First();
            var duplicates = adminUsers.Skip(1).ToList();
            
            Console.WriteLine($"Keeping admin: {keepAdmin.Id} (Created: {keepAdmin.CreatedDate})");
            
            // Remove duplicates
            foreach (var duplicate in duplicates)
            {
                Console.WriteLine($"Removing duplicate admin: {duplicate.Id} (Created: {duplicate.CreatedDate})");
                
                // Delete associated Admin profile
                var adminProfile = await context.Admins
                    .FirstOrDefaultAsync(a => a.UserId == duplicate.Id);
                
                if (adminProfile != null)
                {
                    context.Admins.Remove(adminProfile);
                    Console.WriteLine($"  - Removed Admin profile for user {duplicate.Id}");
                }
                
                // Delete the user
                var result = await userManager.DeleteAsync(duplicate);
                if (result.Succeeded)
                {
                    Console.WriteLine($"  - Successfully deleted user {duplicate.Id}");
                }
                else
                {
                    Console.WriteLine($"  - Failed to delete user {duplicate.Id}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            
            await context.SaveChangesAsync();
            Console.WriteLine("Duplicate admin cleanup completed.");
        }
        
        /// <summary>
        /// Check and report duplicate users by email
        /// </summary>
        public static async Task<Dictionary<string, int>> CheckDuplicateEmails(UserManager<ApplicationUser> userManager)
        {
            var duplicates = await userManager.Users
                .Where(u => u.Email != null)
                .GroupBy(u => u.Email)
                .Where(g => g.Count() > 1)
                .Select(g => new { Email = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Email!, x => x.Count);
            
            if (duplicates.Any())
            {
                Console.WriteLine("Found duplicate emails:");
                foreach (var dup in duplicates)
                {
                    Console.WriteLine($"  - {dup.Key}: {dup.Value} accounts");
                }
            }
            else
            {
                Console.WriteLine("No duplicate emails found.");
            }
            
            return duplicates;
        }
    }
}

