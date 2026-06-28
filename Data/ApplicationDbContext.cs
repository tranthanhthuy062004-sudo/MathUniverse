﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MathUniverse.Data
{
    public class ApplicationDbContext : IdentityDbContext<Models.ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Models.Student> Students { get; set; }
        public DbSet<Models.Admin> Admins { get; set; }
        public DbSet<Models.Lesson> Lessons { get; set; }
        public DbSet<Models.Exercise> Exercises { get; set; }
        public DbSet<Models.Question> Questions { get; set; }
        public DbSet<Models.Answer> Answers { get; set; }
        public DbSet<Models.StudentProgress> StudentProgress { get; set; }
        public DbSet<Models.ExerciseResult> ExerciseResults { get; set; }
        public DbSet<Models.Notification> Notifications { get; set; }
        public DbSet<Models.ActivityLog> ActivityLogs { get; set; }
        public DbSet<Models.GameContent> GameContents { get; set; }
        public DbSet<Models.EssayAnswer> EssayAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Models.Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Models.Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.Admin>()
                .HasOne(a => a.User)
                .WithOne(u => u.Admin)
                .HasForeignKey<Models.Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.Exercise>()
                .HasOne(e => e.Lesson)
                .WithMany(l => l.Exercises)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.Question>()
                .HasOne(q => q.Exercise)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.StudentProgress>()
                .HasOne(sp => sp.Student)
                .WithMany(s => s.Progress)
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.StudentProgress>()
                .HasOne(sp => sp.Lesson)
                .WithMany(l => l.StudentProgress)
                .HasForeignKey(sp => sp.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.ExerciseResult>()
                .HasOne(er => er.Student)
                .WithMany(s => s.ExerciseResults)
                .HasForeignKey(er => er.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Models.ExerciseResult>()
                .HasOne(er => er.Exercise)
                .WithMany(e => e.Results)
                .HasForeignKey(er => er.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Models.Notification>()
                .HasOne(n => n.Student)
                .WithMany(s => s.Notifications)
                .HasForeignKey(n => n.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.GameContent>()
                .HasOne(gc => gc.Lesson)
                .WithMany(l => l.GameContents)
                .HasForeignKey(gc => gc.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes
            modelBuilder.Entity<Models.Student>()
                .HasIndex(s => s.StudentCode)
                .IsUnique();

            modelBuilder.Entity<Models.Lesson>()
                .HasIndex(l => new { l.Grade, l.OrderIndex });

            modelBuilder.Entity<Models.StudentProgress>()
                .HasIndex(sp => new { sp.StudentId, sp.LessonId })
                .IsUnique();
        }
    }
}

