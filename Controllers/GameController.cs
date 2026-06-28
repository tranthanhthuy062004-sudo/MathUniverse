﻿﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MathUniverse.Data;
using MathUniverse.Models;

namespace MathUniverse.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GameController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<Student?> GetCurrentStudentAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            return await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);
        }

        public async Task<IActionResult> MemoryGame(int lessonId)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null) return NotFound();

            // Kiểm tra: Chỉ cho phép chơi game của lớp hiện tại
            if (lesson.Grade != student.Grade)
            {
                TempData["ErrorMessage"] = $"Bạn chỉ có thể chơi trò chơi của lớp {student.Grade}.";
                return RedirectToAction("Lessons", "Student");
            }

            // Check if student has completed watching the video (90%)
            var progress = await _context.StudentProgress
                .FirstOrDefaultAsync(sp => sp.StudentId == student.StudentId && sp.LessonId == lessonId);

            if (progress == null || progress.CompletionPercentage < 90)
            {
                TempData["ErrorMessage"] = "Bạn cần xem ít nhất 90% video bài giảng để mở khóa trò chơi.";
                return RedirectToAction("LessonDetail", "Student", new { id = lessonId });
            }

            ViewBag.LessonTitle = lesson.Title ?? "Trò chơi";
            ViewBag.LessonId = lessonId;

            // Lấy nội dung trò chơi từ database
            var gameContents = await _context.GameContents
                .Where(gc => gc.LessonId == lessonId)
                .OrderBy(gc => gc.OrderIndex)
                .ToListAsync();

            // Nếu chưa có nội dung trò chơi, yêu cầu admin tạo
            if (!gameContents.Any())
            {
                TempData["ErrorMessage"] = "Trò chơi chưa được tạo cho bài học này. Vui lòng liên hệ giáo viên.";
                return RedirectToAction("LessonDetail", "Student", new { id = lessonId });
            }

            // Transform into card deck
            var deck = new List<MemoryCard>();
            int pairId = 1;

            foreach (var gc in gameContents)
            {
                deck.Add(new MemoryCard { Content = gc.CardQuestion, MatchId = pairId, Type = "Question" });
                deck.Add(new MemoryCard { Content = gc.CardAnswer, MatchId = pairId, Type = "Answer" });
                pairId++;
            }

            // Shuffle the deck
            var random = new Random();
            deck = deck.OrderBy(x => random.Next()).ToList();

            return View(deck);
        }


        [HttpPost]
        public async Task<IActionResult> SaveGameScore(int lessonId, int timeSpent, int moves)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Json(new { success = false });

            // Award points for completing the game
            int pointsEarned = Math.Max(10, 50 - moves); // Base points, reduced by moves
            student.TotalPoints += pointsEarned;

            await _context.SaveChangesAsync();

            return Json(new { success = true, pointsEarned });
        }
    }
}

