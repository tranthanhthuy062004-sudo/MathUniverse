﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MathUniverse.Data;
using MathUniverse.Models;
using MathUniverse.Models.ViewModels;
using MathUniverse.Services;
using MathUniverse.Utilities;

namespace MathUniverse.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILessonService _lessonService;
        private readonly IExerciseService _exerciseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            ILessonService lessonService,
            IExerciseService exerciseService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _lessonService = lessonService;
            _exerciseService = exerciseService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                ActiveStudents = await _context.Students.CountAsync(s => s.IsActive),
                TotalLessons = await _context.Lessons.CountAsync(),
                TotalExercises = await _context.Exercises.CountAsync(),
                StudentsByGrade = await _context.Students
                    .GroupBy(s => s.Grade)
                    .ToDictionaryAsync(g => g.Key, g => g.Count())
            };

            return View(viewModel);
        }

        // Student Management
        public async Task<IActionResult> Students(int? grade, string? search)
        {
            var query = _context.Students.AsQueryable();

            if (grade.HasValue)
            {
                query = query.Where(s => s.Grade == grade.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.FullName.Contains(search) || s.StudentCode.Contains(search));
            }

            var students = await query
                .OrderBy(s => s.Grade)
                .ThenBy(s => s.FullName)
                .ToListAsync();

            var viewModel = new ManageStudentViewModel
            {
                Students = students,
                FilterGrade = grade,
                SearchTerm = search
            };

            return View(viewModel);
        }

        public async Task<IActionResult> StudentDetail(int id)
        {
            var student = await _context.Students
                .Include(s => s.Progress)
                    .ThenInclude(p => p.Lesson)
                .Include(s => s.ExerciseResults)
                    .ThenInclude(er => er.Exercise)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null) return NotFound();

            var statistics = new StudentStatistics
            {
                TotalLessonsCompleted = await _context.StudentProgress
                    .CountAsync(sp => sp.StudentId == id && sp.Status == ProgressStatus.Passed),
                TotalExercisesPassed = await _context.ExerciseResults
                    .Where(er => er.StudentId == id && er.IsPassed)
                    .Select(er => er.ExerciseId)
                    .Distinct()
                    .CountAsync(),
                AverageScore = await _context.ExerciseResults
                    .Where(er => er.StudentId == id)
                    .AverageAsync(er => (double?)er.Score) ?? 0,
                TotalPoints = student.TotalPoints,
                BadgesEarned = student.BadgesEarned
            };

            var viewModel = new StudentDetailViewModel
            {
                Student = student,
                Statistics = statistics,
                RecentResults = student.ExerciseResults
                    .OrderByDescending(er => er.CompletedDate)
                    .ToList() // Hiển thị toàn bộ lịch sử, không giới hạn
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStudentStatus(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return Json(new { success = false });

            student.IsActive = !student.IsActive;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = student.IsActive });
        }

        public async Task<IActionResult> ViewExerciseResult(int resultId)
        {
            var result = await _context.ExerciseResults
                .Include(er => er.Student)
                .Include(er => er.Exercise)
                    .ThenInclude(e => e.Lesson)
                .Include(er => er.Exercise)
                    .ThenInclude(e => e.Questions)
                        .ThenInclude(q => q.Answers)
                .Include(er => er.EssayAnswers)
                    .ThenInclude(ea => ea.Question)
                .FirstOrDefaultAsync(er => er.ResultId == resultId);

            if (result == null) return NotFound();

            return View(result);
        }

        // Lesson Management
        public async Task<IActionResult> Lessons()
        {
            var lessons = await _lessonService.GetAllLessonsAsync();
            return View(lessons);
        }

        [HttpGet]
        public IActionResult CreateLesson()
        {
            return View();
        }

        private string? ConvertToEmbedUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return url;

            string videoId = string.Empty;

            if (url.Contains("youtu.be/"))
            {
                videoId = url.Split("youtu.be/")[1].Split('?')[0];
            }
            else if (url.Contains("youtube.com/embed/"))
            {
                return url; // Already embed url
            }
            else if (url.Contains("v="))
            {
                var parts = url.Split("v=");
                if (parts.Length > 1)
                {
                    videoId = parts[1].Split('&')[0];
                }
            }

            if (!string.IsNullOrEmpty(videoId))
            {
                return $"https://www.youtube.com/embed/{videoId}";
            }

            return url;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLesson(CreateLessonViewModel model)
        {
            // Validate: At least one content type (Video or PDF) is required
            if (string.IsNullOrWhiteSpace(model.VideoUrl) && model.PdfFile == null)
            {
                ModelState.AddModelError("", "Vui lòng cung cấp ít nhất một trong hai: Video YouTube hoặc File PDF");
            }

            if (ModelState.IsValid)
            {
                // Handle PDF file upload
                string? pdfUrl = null;
                if (model.PdfFile != null && model.PdfFile.Length > 0)
                {
                    pdfUrl = await SavePdfFileAsync(model.PdfFile);
                }

                var lesson = new Lesson
                {
                    Title = model.Title,
                    Description = model.Description,
                    Grade = model.Grade,
                    Topic = model.Topic,
                    VideoUrl = ConvertToEmbedUrl(model.VideoUrl),
                    PdfUrl = pdfUrl,
                    TheoryContent = model.TheoryContent,
                    ThumbnailUrl = model.ThumbnailUrl,
                    OrderIndex = model.OrderIndex,
                    IsPublished = model.IsPublished,
                    PreviousLessonId = model.PreviousLessonId,
                    CreatedDate = DateTime.Now
                };

                await _lessonService.CreateLessonAsync(lesson);
                TempData["SuccessMessage"] = "Tạo bài học thành công!";
                return RedirectToAction(nameof(Lessons));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(id);
            if (lesson == null) return NotFound();

            var model = new CreateLessonViewModel
            {
                Title = lesson.Title,
                Description = lesson.Description,
                Grade = lesson.Grade,
                Topic = lesson.Topic,
                VideoUrl = lesson.VideoUrl,
                ExistingPdfUrl = lesson.PdfUrl,
                TheoryContent = lesson.TheoryContent,
                ThumbnailUrl = lesson.ThumbnailUrl,
                OrderIndex = lesson.OrderIndex,
                IsPublished = lesson.IsPublished,
                PreviousLessonId = lesson.PreviousLessonId
            };

            ViewBag.LessonId = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(int id, CreateLessonViewModel model)
        {
            // Validate: At least one content type (Video or PDF) is required
            if (string.IsNullOrWhiteSpace(model.VideoUrl) && model.PdfFile == null && string.IsNullOrWhiteSpace(model.ExistingPdfUrl))
            {
                ModelState.AddModelError("", "Vui lòng cung cấp ít nhất một trong hai: Video YouTube hoặc File PDF");
            }

            if (ModelState.IsValid)
            {
                var lesson = await _lessonService.GetLessonByIdAsync(id);
                if (lesson == null) return NotFound();

                // Handle PDF file upload (if new file provided)
                if (model.PdfFile != null && model.PdfFile.Length > 0)
                {
                    // Delete old PDF if exists
                    if (!string.IsNullOrEmpty(lesson.PdfUrl))
                    {
                        DeletePdfFile(lesson.PdfUrl);
                    }
                    lesson.PdfUrl = await SavePdfFileAsync(model.PdfFile);
                }

                lesson.Title = model.Title;
                lesson.Description = model.Description;
                lesson.Grade = model.Grade;
                lesson.Topic = model.Topic;
                lesson.VideoUrl = ConvertToEmbedUrl(model.VideoUrl);
                lesson.TheoryContent = model.TheoryContent;
                lesson.ThumbnailUrl = model.ThumbnailUrl;
                lesson.OrderIndex = model.OrderIndex;
                lesson.IsPublished = model.IsPublished;
                lesson.PreviousLessonId = model.PreviousLessonId;
                lesson.UpdatedDate = DateTime.Now;

                await _lessonService.UpdateLessonAsync(lesson);
                TempData["SuccessMessage"] = "Cập nhật bài học thành công!";
                return RedirectToAction(nameof(Lessons));
            }

            ViewBag.LessonId = id;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var result = await _lessonService.DeleteLessonAsync(id);
            return Json(new { success = result });
        }

        // Exercise Management
        public async Task<IActionResult> Exercises(int? lessonId)
        {
            IEnumerable<Exercise> exercises;

            if (lessonId.HasValue)
            {
                exercises = await _exerciseService.GetExercisesByLessonAsync(lessonId.Value);
                var lesson = await _lessonService.GetLessonByIdAsync(lessonId.Value);
                ViewBag.LessonTitle = lesson?.Title;
            }
            else
            {
                exercises = await _context.Exercises
                    .Include(e => e.Lesson)
                    .Include(e => e.Questions)
                    .OrderBy(e => e.Lesson.Grade)
                    .ThenBy(e => e.Lesson.OrderIndex)
                    .ThenBy(e => e.OrderIndex)
                    .ToListAsync();
            }

            ViewBag.LessonId = lessonId;
            return View(exercises);
        }

        [HttpGet]
        public async Task<IActionResult> CreateExercise(int lessonId)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
            if (lesson == null) return NotFound();

            var model = new CreateExerciseViewModel
            {
                LessonId = lessonId
            };

            ViewBag.LessonTitle = lesson.Title;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExercise(CreateExerciseViewModel model)
        {
            // Log for debugging
            Console.WriteLine($"[CreateExercise POST] LessonId: {model.LessonId}, Title: {model.Title}, Questions Count: {model.Questions?.Count ?? 0}");
            
            // Check if Questions is null or empty
            if (model.Questions == null || !model.Questions.Any())
            {
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một câu hỏi cho bài tập.");
            }
            
            // Log ModelState errors for debugging
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"[CreateExercise POST] ModelState Error: {error.ErrorMessage}");
                }
            }
            
            if (ModelState.IsValid)
            {
                var exercise = new Exercise
                {
                    LessonId = model.LessonId,
                    Title = model.Title,
                    Description = model.Description,
                    Type = model.Type,
                    Difficulty = model.Difficulty,
                    MaxAttempts = model.MaxAttempts,
                    PassingScore = model.PassingScore,
                    TimeLimit = model.TimeLimit,
                    OrderIndex = model.OrderIndex,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                // Helper to ensure valid Enum values (since 0 is not a valid named constant in our Enums)
                if (!Enum.IsDefined(typeof(ExerciseType), exercise.Type))
                    exercise.Type = ExerciseType.MultipleChoice;
                
                if (!Enum.IsDefined(typeof(DifficultyLevel), exercise.Difficulty))
                    exercise.Difficulty = DifficultyLevel.Easy;

                // Add questions and answers
                foreach (var questionModel in model.Questions)
                {
                    var question = new Question
                    {
                        Type = questionModel.Type,  // Thêm Type để phân biệt trắc nghiệm/tự luận
                        QuestionText = questionModel.QuestionText,
                        ImageUrl = questionModel.ImageUrl,
                        AudioUrl = questionModel.AudioUrl,
                        Points = questionModel.Points,
                        OrderIndex = questionModel.OrderIndex
                    };

                    // Only add answers for Multiple Choice questions (Type = 1)
                    if (questionModel.Type == QuestionType.MultipleChoice && questionModel.Answers != null)
                    {
                        foreach (var answerModel in questionModel.Answers)
                        {
                            // Skip empty answers
                            if (string.IsNullOrWhiteSpace(answerModel.AnswerText)) continue;

                            question.Answers.Add(new Answer
                            {
                                AnswerText = answerModel.AnswerText ?? string.Empty,
                                ImageUrl = answerModel.ImageUrl,
                                IsCorrect = answerModel.IsCorrect,
                                OrderIndex = answerModel.OrderIndex,
                                Explanation = answerModel.Explanation
                            });
                        }
                    }
                    // For Essay questions (Type = 2), no answers are added

                    exercise.Questions.Add(question);
                }

                Console.WriteLine($"[CreateExercise POST] Creating exercise with {exercise.Questions.Count} questions");
                await _exerciseService.CreateExerciseAsync(exercise);
                Console.WriteLine($"[CreateExercise POST] Exercise created successfully with ID: {exercise.ExerciseId}");
                
                TempData["SuccessMessage"] = "Tạo bài tập thành công!";
                // Redirect to Exercises list for this lesson
                return RedirectToAction(nameof(Exercises), new { lessonId = model.LessonId });
            }

            // If we got here, ModelState is invalid
            var lesson = await _lessonService.GetLessonByIdAsync(model.LessonId);
            ViewBag.LessonTitle = lesson?.Title;
            
            // Add a user-friendly error message
            if (!ModelState.Values.SelectMany(v => v.Errors).Any())
            {
                ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng kiểm tra lại thông tin.");
            }
            
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditExercise(int id)
        {
            var exercise = await _exerciseService.GetExerciseByIdAsync(id);
            if (exercise == null) return NotFound();

            var model = new CreateExerciseViewModel
            {
                LessonId = exercise.LessonId,
                Title = exercise.Title,
                Description = exercise.Description,
                Type = exercise.Type,
                Difficulty = exercise.Difficulty,
                MaxAttempts = exercise.MaxAttempts,
                PassingScore = exercise.PassingScore,
                TimeLimit = exercise.TimeLimit,
                OrderIndex = exercise.OrderIndex,
                Questions = exercise.Questions.Select(q => new CreateQuestionViewModel
                {
                    QuestionText = q.QuestionText,
                    ImageUrl = q.ImageUrl,
                    AudioUrl = q.AudioUrl,
                    Points = q.Points,
                    OrderIndex = q.OrderIndex,
                    Answers = q.Answers.Select(a => new CreateAnswerViewModel
                    {
                        AnswerText = a.AnswerText,
                        ImageUrl = a.ImageUrl,
                        IsCorrect = a.IsCorrect,
                        OrderIndex = a.OrderIndex,
                        Explanation = a.Explanation
                    }).ToList()
                }).ToList()
            };

            ViewBag.ExerciseId = id;
            ViewBag.LessonTitle = exercise.Lesson.Title;
            return View("CreateExercise", model); // Reuse CreateExercise view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExercise(int id, CreateExerciseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exercise = await _exerciseService.GetExerciseByIdAsync(id);
                if (exercise == null) return NotFound();

                // Update basic info
                exercise.Title = model.Title;
                exercise.Description = model.Description;
                exercise.Type = model.Type;
                exercise.Difficulty = model.Difficulty;
                exercise.MaxAttempts = model.MaxAttempts;
                exercise.PassingScore = model.PassingScore;
                exercise.TimeLimit = model.TimeLimit;
                exercise.OrderIndex = model.OrderIndex;

                // Remove old questions
                _context.Questions.RemoveRange(exercise.Questions);

                // Add new questions
                exercise.Questions.Clear();
                foreach (var questionModel in model.Questions)
                {
                    var question = new Question
                    {
                        Type = questionModel.Type,  // Thêm Type
                        QuestionText = questionModel.QuestionText,
                        ImageUrl = questionModel.ImageUrl,
                        AudioUrl = questionModel.AudioUrl,
                        Points = questionModel.Points,
                        OrderIndex = questionModel.OrderIndex
                    };

                    foreach (var answerModel in questionModel.Answers)
                    {
                        question.Answers.Add(new Answer
                        {
                            AnswerText = answerModel.AnswerText,
                            ImageUrl = answerModel.ImageUrl,
                            IsCorrect = answerModel.IsCorrect,
                            OrderIndex = answerModel.OrderIndex,
                            Explanation = answerModel.Explanation
                        });
                    }

                    exercise.Questions.Add(question);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật bài tập thành công!";
                return RedirectToAction(nameof(Exercises), new { lessonId = model.LessonId });
            }

            var lesson = await _lessonService.GetLessonByIdAsync(model.LessonId);
            ViewBag.ExerciseId = id;
            ViewBag.LessonTitle = lesson?.Title;
            return View("CreateExercise", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            try
            {
                var exercise = await _exerciseService.GetExerciseByIdAsync(id);
                if (exercise == null) return Json(new { success = false });

                _context.Exercises.Remove(exercise);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        // Reports
        public async Task<IActionResult> Reports()
        {
            var recentActivity = await _context.ExerciseResults
                .Include(er => er.Student)
                .Include(er => er.Exercise)
                .OrderByDescending(er => er.CompletedDate)
                .Take(20)
                .ToListAsync();

            return View(recentActivity);
        }


        // ==================== QUẢN LÝ TRÒ CHƠI ====================
        
        [HttpGet]
        public async Task<IActionResult> Games()
        {
            var lessons = await _context.Lessons
                .Include(l => l.GameContents)
                .OrderBy(l => l.Grade)
                .ThenBy(l => l.OrderIndex)
                .ToListAsync();

            return View(lessons);
        }

        [HttpGet]
        public async Task<IActionResult> ManageGameContent(int lessonId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.GameContents)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null) return NotFound();

            ViewBag.LessonId = lessonId;
            ViewBag.LessonTitle = lesson.Title;

            return View(lesson.GameContents.OrderBy(gc => gc.OrderIndex).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGameContent(int lessonId, string cardQuestion, string cardAnswer)
        {
            if (string.IsNullOrWhiteSpace(cardQuestion) || string.IsNullOrWhiteSpace(cardAnswer))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ câu hỏi và đáp án!";
                return RedirectToAction(nameof(ManageGameContent), new { lessonId });
            }

            var maxOrder = await _context.GameContents
                .Where(gc => gc.LessonId == lessonId)
                .MaxAsync(gc => (int?)gc.OrderIndex) ?? 0;

            var gameContent = new GameContent
            {
                LessonId = lessonId,
                CardQuestion = cardQuestion.Trim(),
                CardAnswer = cardAnswer.Trim(),
                OrderIndex = maxOrder + 1,
                CreatedDate = DateTime.Now
            };

            _context.GameContents.Add(gameContent);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm cặp thẻ mới!";
            return RedirectToAction(nameof(ManageGameContent), new { lessonId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGameContent(int gameContentId, string cardQuestion, string cardAnswer)
        {
            var gameContent = await _context.GameContents.FindAsync(gameContentId);
            if (gameContent == null) return NotFound();

            if (string.IsNullOrWhiteSpace(cardQuestion) || string.IsNullOrWhiteSpace(cardAnswer))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ câu hỏi và đáp án!";
                return RedirectToAction(nameof(ManageGameContent), new { lessonId = gameContent.LessonId });
            }

            gameContent.CardQuestion = cardQuestion.Trim();
            gameContent.CardAnswer = cardAnswer.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật cặp thẻ!";
            return RedirectToAction(nameof(ManageGameContent), new { lessonId = gameContent.LessonId });
        }


        // ==================== UPLOAD ẢNH ====================
        
        [HttpPost]
        public async Task<IActionResult> UploadQuestionImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Không có file được chọn" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)" });
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return Json(new { success = false, message = "Kích thước file không được vượt quá 5MB" });
            }

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "questions");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/questions/{fileName}";
                return Json(new { success = true, imageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tải lên: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadEssayImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Không có file được chọn" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)" });
            }

            // Validate file size (max 10MB for essay images)
            if (file.Length > 10 * 1024 * 1024)
            {
                return Json(new { success = false, message = "Kích thước file không được vượt quá 10MB" });
            }

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "essays");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/essays/{fileName}";
                return Json(new { success = true, imageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi tải lên: {ex.Message}" });
            }
        }

        // ==================== CHẤM ĐIỂM TỰ LUẬN ====================
        
        [HttpGet]
        public async Task<IActionResult> EssayGrading(int? lessonId, bool? ungraded)
        {
            var query = _context.EssayAnswers
                .Include(ea => ea.ExerciseResult)
                    .ThenInclude(er => er.Student)
                .Include(ea => ea.ExerciseResult)
                    .ThenInclude(er => er.Exercise)
                        .ThenInclude(e => e.Lesson)
                .Include(ea => ea.Question)
                .AsQueryable();

            // Filter by lesson
            if (lessonId.HasValue)
            {
                query = query.Where(ea => ea.ExerciseResult.Exercise.LessonId == lessonId.Value);
            }

            // Filter by grading status
            if (ungraded == true)
            {
                query = query.Where(ea => ea.Score == null);
            }

            var essayAnswers = await query
                .OrderByDescending(ea => ea.SubmittedDate)
                .ToListAsync();

            ViewBag.LessonId = lessonId;
            ViewBag.Ungraded = ungraded;
            ViewBag.Lessons = await _context.Lessons.OrderBy(l => l.Grade).ThenBy(l => l.OrderIndex).ToListAsync();

            return View(essayAnswers);
        }

        [HttpPost]
        public async Task<IActionResult> GradeEssay(int essayAnswerId, double score, string? feedback)
        {
            try
            {
                var essayAnswer = await _context.EssayAnswers
                    .Include(ea => ea.ExerciseResult)
                        .ThenInclude(er => er.Student)
                    .Include(ea => ea.ExerciseResult)
                        .ThenInclude(er => er.Exercise)
                            .ThenInclude(e => e.Questions)
                    .Include(ea => ea.Question)
                    .FirstOrDefaultAsync(ea => ea.EssayAnswerId == essayAnswerId);

                if (essayAnswer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài làm" });
                }

                // Validate score
                if (score < 0 || score > 10)
                {
                    return Json(new { success = false, message = "Điểm phải từ 0 đến 10" });
                }

                var user = await _userManager.GetUserAsync(User);
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.UserId == user.Id);

                essayAnswer.Score = score;
                essayAnswer.Feedback = feedback;
                essayAnswer.GradedDate = DateTime.Now;
                essayAnswer.GradedByAdminId = admin?.AdminId;

                // Update ExerciseResult score
                var exerciseResult = essayAnswer.ExerciseResult;
                var exercise = exerciseResult.Exercise;
                
                var allEssayAnswers = await _context.EssayAnswers
                    .Where(ea => ea.ExerciseResultId == exerciseResult.ResultId)
                    .ToListAsync();

                // Check if all essay questions are graded
                var gradedAnswers = allEssayAnswers.Where(ea => ea.Score.HasValue).ToList();
                
                if (gradedAnswers.Count == allEssayAnswers.Count)
                {
                    // All essay questions graded - calculate final score
                    var multipleChoiceQuestions = exercise.Questions.Where(q => q.Type == QuestionType.MultipleChoice).ToList();
                    var essayQuestions = exercise.Questions.Where(q => q.Type == QuestionType.Essay).ToList();
                    
                    double finalScore = 0;
                    
                    if (multipleChoiceQuestions.Any() && essayQuestions.Any())
                    {
                        // Mix of both types - weight equally
                        var mcScore = exerciseResult.Score; // Already calculated from MC questions
                        var essayScore = gradedAnswers.Average(ea => ea.Score!.Value);
                        
                        var totalQuestions = multipleChoiceQuestions.Count + essayQuestions.Count;
                        finalScore = (mcScore * multipleChoiceQuestions.Count + essayScore * essayQuestions.Count) / totalQuestions;
                    }
                    else if (essayQuestions.Any())
                    {
                        // Only essay questions
                        finalScore = gradedAnswers.Average(ea => ea.Score!.Value);
                    }
                    else
                    {
                        // Only MC (shouldn't happen but fallback)
                        finalScore = exerciseResult.Score;
                    }
                    
                    exerciseResult.Score = Math.Round(finalScore, 2);
                    exerciseResult.IsPassed = finalScore >= exercise.PassingScore;
                    exerciseResult.GradingStatus = GradingStatus.Graded;
                    exerciseResult.TotalQuestions = multipleChoiceQuestions.Count + essayQuestions.Count;
                    
                    // Save changes first to update ExerciseResult
                    await _context.SaveChangesAsync();
                    
                    // Recalculate student total points (this will save again internally)
                    await _exerciseService.RecalculateStudentTotalPointsAsync(exerciseResult.StudentId);
                    
                    // Send notification to student
                    var notificationService = HttpContext.RequestServices.GetService<INotificationService>();
                    if (notificationService != null)
                    {
                        var passedMessage = exerciseResult.IsPassed
                            ? $"🎉 Chúc mừng! Bạn đã hoàn thành bài tập '{exercise.Title}' với điểm số {finalScore:F1}/10. Tuyệt vời! 💪"
                            : $"Bài tập '{exercise.Title}' đã được chấm điểm: {finalScore:F1}/10. Hãy cố gắng thêm nhé! 📚";
                        
                        await notificationService.CreateNotificationAsync(
                            exerciseResult.StudentId,
                            exerciseResult.IsPassed ? "✅ Bài tập đã được chấm điểm!" : "📝 Bài tập đã được chấm điểm",
                            passedMessage,
                            exerciseResult.IsPassed ? NotificationType.Achievement : NotificationType.System,
                            $"/Student/LessonDetail/{exercise.LessonId}"
                        );
                        
                        // If passed, check and unlock next lesson
                        if (exerciseResult.IsPassed)
                        {
                            var progressService = HttpContext.RequestServices.GetService<IStudentProgressService>();
                            if (progressService != null)
                            {
                                await progressService.MarkLessonCompletedAsync(exerciseResult.StudentId, exercise.LessonId);
                                await progressService.CheckAndUnlockNextLessonAsync(exerciseResult.StudentId, exercise.LessonId);
                            }
                        }
                    }
                }
                else
                {
                    // Not all questions graded yet, just save the current essay answer
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Chấm điểm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ==================== HELPER METHODS ====================
        
        private async Task<string> SavePdfFileAsync(IFormFile pdfFile)
        {
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "pdfs");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(pdfFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await pdfFile.CopyToAsync(stream);
                }

                return $"/uploads/pdfs/{fileName}";
            }
            catch (Exception)
            {
                throw new Exception("Lỗi khi tải lên file PDF");
            }
        }

        private void DeletePdfFile(string pdfUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(pdfUrl))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", pdfUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            catch
            {
                // Ignore errors when deleting files
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAllStudents()
        {
            try
            {
                await DatabaseCleanup.DeleteAllStudentAccounts(_userManager, _context);
                TempData["SuccessMessage"] = "Đã xóa tất cả tài khoản học sinh thành công!";
                return Json(new { success = true, message = "Đã xóa tất cả tài khoản học sinh!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}

