using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MathUniverse.Data;
using MathUniverse.Models;
using MathUniverse.Models.ViewModels;
using MathUniverse.Services;

namespace MathUniverse.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILessonService _lessonService;
        private readonly IStudentProgressService _progressService;
        private readonly IExerciseService _exerciseService;
        private readonly INotificationService _notificationService;

        public StudentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILessonService lessonService,
            IStudentProgressService progressService,
            IExerciseService exerciseService,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _lessonService = lessonService;
            _progressService = progressService;
            _exerciseService = exerciseService;
            _notificationService = notificationService;
        }

        private async Task<Student?> GetCurrentStudentAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            return await _context.Students
                .Include(s => s.Progress)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);
        }

        public async Task<IActionResult> Dashboard()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            var recentLessons = await _context.StudentProgress
                .Where(sp => sp.StudentId == student.StudentId && 
                            sp.Status != ProgressStatus.NotStarted &&
                            !sp.Lesson.IsDeleted)  // Lọc bài giảng đã xóa
                .OrderByDescending(sp => sp.LastAccessedDate)
                .Take(5)
                .Include(sp => sp.Lesson)
                .Select(sp => new LessonViewModel
                {
                    LessonId = sp.Lesson.LessonId,
                    Title = sp.Lesson.Title,
                    Description = sp.Lesson.Description,
                    Grade = sp.Lesson.Grade,
                    Topic = sp.Lesson.Topic,
                    ThumbnailUrl = sp.Lesson.ThumbnailUrl,
                    CompletionPercentage = sp.CompletionPercentage,
                    Status = sp.Status
                })
                .ToListAsync();

            var notifications = await _notificationService.GetStudentNotificationsAsync(student.StudentId);

            var statistics = new StudentStatistics
            {
                TotalLessonsCompleted = await _context.StudentProgress
                    .CountAsync(sp => sp.StudentId == student.StudentId && sp.Status == ProgressStatus.Passed),
                TotalExercisesPassed = await _context.ExerciseResults
                    .Where(er => er.StudentId == student.StudentId && er.IsPassed)
                    .Select(er => er.ExerciseId)
                    .Distinct()
                    .CountAsync(),
                AverageScore = await _context.ExerciseResults
                    .Where(er => er.StudentId == student.StudentId)
                    .AverageAsync(er => (double?)er.Score) ?? 0,
                TotalPoints = student.TotalPoints,
                BadgesEarned = student.BadgesEarned
            };

            var viewModel = new StudentDashboardViewModel
            {
                Student = student,
                RecentLessons = recentLessons,
                RecentNotifications = notifications.Take(5).ToList(),
                Statistics = statistics
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Lessons(int? grade)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            // Cho phép xem tất cả các lớp, nhưng chỉ mở khóa lớp hiện tại
            var selectedGrade = grade ?? student.Grade;

            // Tự động kiểm tra và mở khóa các bài học đủ điều kiện
            await _progressService.CheckAndUnlockAllEligibleLessonsAsync(student.StudentId, student.Grade);

            var lessons = await _lessonService.GetLessonsByGradeAsync(selectedGrade);

            var progressList = await _progressService.GetStudentProgressAsync(student.StudentId);
            var progressDict = progressList.ToDictionary(p => p.LessonId);

            var viewModel = lessons.Select(l => new LessonViewModel
            {
                LessonId = l.LessonId,
                Title = l.Title,
                Description = l.Description,
                Grade = l.Grade,
                Topic = l.Topic,
                VideoUrl = l.VideoUrl,
                VideoDuration = l.VideoDuration,
                ThumbnailUrl = l.ThumbnailUrl,
                OrderIndex = l.OrderIndex,
                ExerciseCount = l.Exercises.Count,
                // Chỉ mở khóa nếu là lớp hiện tại VÀ có trong progress
                IsUnlocked = (l.Grade == student.Grade) && progressDict.ContainsKey(l.LessonId) &&
                             progressDict[l.LessonId].IsUnlocked,
                CompletionPercentage = progressDict.ContainsKey(l.LessonId)
                    ? progressDict[l.LessonId].CompletionPercentage
                    : 0,
                Status = progressDict.ContainsKey(l.LessonId)
                    ? progressDict[l.LessonId].Status
                    : ProgressStatus.NotStarted
            }).ToList();

            ViewBag.SelectedGrade = selectedGrade;
            ViewBag.StudentGrade = student.Grade;

            return View(viewModel);
        }

        public async Task<IActionResult> LessonDetail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var lesson = await _lessonService.GetLessonByIdAsync(id);
            if (lesson == null) return NotFound();

            // Check if user is Admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                // Admin can view all lessons
                var exercises = await _exerciseService.GetExercisesByLessonAsync(id);
                var exerciseViewModels = new List<ExerciseViewModel>();

                foreach (var exercise in exercises)
                {
                    exerciseViewModels.Add(new ExerciseViewModel
                    {
                        ExerciseId = exercise.ExerciseId,
                        Title = exercise.Title,
                        Description = exercise.Description,
                        Type = exercise.Type,
                        Difficulty = exercise.Difficulty,
                        MaxAttempts = exercise.MaxAttempts,
                        PassingScore = exercise.PassingScore,
                        TimeLimit = exercise.TimeLimit,
                        QuestionCount = exercise.Questions.Count,
                        AttemptsUsed = 0,
                        BestScore = null,
                        IsPassed = false
                    });
                }

                var viewModel = new LessonDetailViewModel
                {
                    Lesson = lesson,
                    Progress = null,
                    Exercises = exerciseViewModels,
                    CanAccessExercises = true // Admin can access all
                };

                return View(viewModel);
            }

            // Regular student flow
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            // Kiểm tra: Chỉ cho phép xem bài học của lớp hiện tại
            if (lesson.Grade != student.Grade)
            {
                TempData["ErrorMessage"] =
                    $"Bạn chỉ có thể xem bài học của lớp {student.Grade}. Bài học này thuộc lớp {lesson.Grade}.";
                return RedirectToAction("Lessons");
            }

            var progress = await _progressService.GetProgressAsync(student.StudentId, id);

            // Auto-complete lesson if it only has PDF (no video)
            if (string.IsNullOrEmpty(lesson.VideoUrl) && !string.IsNullOrEmpty(lesson.PdfUrl))
            {
                try
                {
                    progress = await _progressService.AutoCompletePdfLessonAsync(student.StudentId, id);
                }
                catch (Exception)
                {
                    // If already completed or error, get current progress
                    progress = await _progressService.GetProgressAsync(student.StudentId, id);
                }
            }

            var exercisesForStudent = await _exerciseService.GetExercisesByLessonAsync(id);
            var exerciseViewModelsForStudent = new List<ExerciseViewModel>();

            foreach (var exercise in exercisesForStudent)
            {
                var results = await _exerciseService.GetStudentResultsAsync(student.StudentId, exercise.ExerciseId);
                exerciseViewModelsForStudent.Add(new ExerciseViewModel
                {
                    ExerciseId = exercise.ExerciseId,
                    Title = exercise.Title,
                    Description = exercise.Description,
                    Type = exercise.Type,
                    Difficulty = exercise.Difficulty,
                    MaxAttempts = exercise.MaxAttempts,
                    PassingScore = exercise.PassingScore,
                    TimeLimit = exercise.TimeLimit,
                    QuestionCount = exercise.Questions.Count,
                    AttemptsUsed = results.Count,
                    BestScore = results.Any() ? results.Max(r => r.Score) : null,
                    IsPassed = results.Any(r => r.IsPassed)
                });
            }

            // Kiểm tra điều kiện access exercises:
            // - Nếu bài học KHÔNG có video (chỉ có PDF), cho phép làm bài tập ngay
            // - Nếu bài học CÓ video, yêu cầu xem hết video (CompletionPercentage >= 100)
            var canAccessExercises = string.IsNullOrEmpty(lesson.VideoUrl) ||
                                     (progress != null && progress.CompletionPercentage >= 100);

            var studentViewModel = new LessonDetailViewModel
            {
                Lesson = lesson,
                Progress = progress,
                Exercises = exerciseViewModelsForStudent,
                CanAccessExercises = canAccessExercises
            };

            return View(studentViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVideoProgress(int lessonId, int watchedSeconds)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Json(new { success = false });

            var result = await _progressService.UpdateVideoProgressAsync(student.StudentId, lessonId, watchedSeconds);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteVideo([FromBody] CompleteVideoRequest request)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Json(new { success = false, message = "Không tìm thấy thông tin học sinh" });

            try
            {
                // Get or create progress record
                var progress = await _context.StudentProgress
                    .FirstOrDefaultAsync(sp => sp.StudentId == student.StudentId && sp.LessonId == request.LessonId);

                if (progress == null)
                {
                    progress = new StudentProgress
                    {
                        StudentId = student.StudentId,
                        LessonId = request.LessonId,
                        Status = ProgressStatus.Completed,
                        CompletionPercentage = 100,
                        IsUnlocked = true,
                        StartedDate = DateTime.Now,
                        CompletedDate = DateTime.Now,
                        LastAccessedDate = DateTime.Now
                    };
                    _context.StudentProgress.Add(progress);
                }
                else
                {
                    // Update existing progress
                    progress.Status = ProgressStatus.Completed;
                    progress.CompletionPercentage = 100;
                    progress.CompletedDate = DateTime.Now;
                    progress.LastAccessedDate = DateTime.Now;
                    _context.StudentProgress.Update(progress);
                }

                await _context.SaveChangesAsync();

                // Tạo notification khi hoàn thành xem video
                var lesson = await _context.Lessons.FindAsync(request.LessonId);
                if (lesson != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        student.StudentId,
                        "📺 Hoàn thành xem video!",
                        $"Tuyệt vời! Bạn đã hoàn thành xem video bài học '{lesson.Title}'. Giờ bạn có thể làm bài tập và chơi game rồi! 🎮",
                        NotificationType.Achievement,
                        $"/Student/LessonDetail/{request.LessonId}"
                    );
                }

                return Json(new { success = true, message = "Đã hoàn thành xem video!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Practice(int lessonId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
            if (lesson == null) return NotFound();

            // Check if user is Admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (!isAdmin)
            {
                // Regular student flow - check requirements
                var student = await GetCurrentStudentAsync();
                if (student == null) return RedirectToAction("Login", "Account");

                // Check if student has watched 100% of video
                var progress = await _context.StudentProgress
                    .FirstOrDefaultAsync(sp => sp.StudentId == student.StudentId && sp.LessonId == lessonId);

                if (progress == null || progress.CompletionPercentage < 100)
                {
                    TempData["ErrorMessage"] = "Bạn cần xem hết video bài giảng trước khi làm bài luyện tập.";
                    return RedirectToAction("LessonDetail", new { id = lessonId });
                }
            }

            // Get 10 random questions from exercises of this lesson
            var allQuestionsFromDb = await _context.Questions
                .Where(q => q.Exercise.LessonId == lessonId)
                .Include(q => q.Answers)
                .ToListAsync();

            // Shuffle in memory and take 10
            var selectedQuestions = allQuestionsFromDb
                .OrderBy(q => Guid.NewGuid())
                .Take(10)
                .ToList();

            if (!selectedQuestions.Any())
            {
                TempData["ErrorMessage"] = "Bài học này chưa có câu hỏi luyện tập.";
                return RedirectToAction("LessonDetail", new { id = lessonId });
            }

            // Get previous results (only for students)
            ExerciseResult? previousBestResult = null;
            if (!isAdmin)
            {
                var student = await GetCurrentStudentAsync();
                if (student != null)
                {
                    previousBestResult = await _context.ExerciseResults
                        .Where(er => er.StudentId == student.StudentId &&
                                     er.Exercise.LessonId == lessonId)
                        .OrderByDescending(er => er.Score)
                        .FirstOrDefaultAsync();
                }
            }

            var practiceViewModel = new PracticeViewModel
            {
                LessonId = lessonId,
                LessonTitle = lesson.Title,
                Grade = lesson.Grade,
                Questions = selectedQuestions.Select((q, index) => new PracticeQuestion
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    ImageUrl = q.ImageUrl,
                    OrderIndex = index + 1,
                    Answers = q.Answers.OrderBy(a => a.OrderIndex).Select(a => new PracticeAnswer
                    {
                        AnswerId = a.AnswerId,
                        AnswerText = a.AnswerText,
                        ImageUrl = a.ImageUrl
                    }).ToList()
                }).ToList(),
                PreviousBestScore = previousBestResult != null ? (int)(previousBestResult.Score * 10) : null,
                HasPassed = previousBestResult != null && previousBestResult.Score >= 7.0
            };

            return View(practiceViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitPractice([FromBody] SubmitPracticeViewModel model)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Json(new { success = false, message = "Không tìm thấy thông tin học sinh" });

            try
            {
                var questionIds = model.Answers.Keys.ToList();
                var questions = await _context.Questions
                    .Where(q => questionIds.Contains(q.QuestionId))
                    .Include(q => q.Answers)
                    .ToListAsync();

                int correctCount = 0;
                var questionResults = new List<PracticeQuestionResult>();

                foreach (var question in questions)
                {
                    var selectedAnswerId = model.Answers.ContainsKey(question.QuestionId)
                        ? model.Answers[question.QuestionId]
                        : 0;

                    var selectedAnswer = question.Answers.FirstOrDefault(a => a.AnswerId == selectedAnswerId);
                    var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);

                    bool isCorrect = selectedAnswer != null && selectedAnswer.IsCorrect;
                    if (isCorrect) correctCount++;

                    questionResults.Add(new PracticeQuestionResult
                    {
                        QuestionText = question.QuestionText,
                        SelectedAnswer = selectedAnswer?.AnswerText ?? "Chưa chọn",
                        CorrectAnswer = correctAnswer?.AnswerText ?? "",
                        IsCorrect = isCorrect,
                        Explanation = correctAnswer?.Explanation
                    });
                }

                int score = (int)((correctCount * 100.0) / questions.Count);
                bool isPassed = score >= 70;

                // Update progress if passed
                if (isPassed)
                {
                    var progress = await _context.StudentProgress
                        .FirstOrDefaultAsync(sp => sp.StudentId == student.StudentId && sp.LessonId == model.LessonId);

                    if (progress != null)
                    {
                        progress.Status = ProgressStatus.Passed;
                        progress.CompletedDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }

                var result = new PracticeResultViewModel
                {
                    Score = score,
                    CorrectAnswers = correctCount,
                    TotalQuestions = questions.Count,
                    IsPassed = isPassed,
                    Message = isPassed
                        ? $"🎉 Chúc mừng! Bạn đã hoàn thành bài học với {score} điểm!"
                        : $"Bạn đạt {score} điểm. Cần ít nhất 70 điểm để hoàn thành bài học. Hãy cố gắng thêm nhé!",
                    QuestionResults = questionResults
                };

                return Json(new { success = true, result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Exercise(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var exercise = await _exerciseService.GetExerciseByIdAsync(id);
            if (exercise == null) return NotFound();

            // Check if user is Admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                // Admin can view all exercises (preview mode)
                var viewModel = new ExerciseDetailViewModel
                {
                    Exercise = exercise,
                    Questions = exercise.Questions.OrderBy(q => q.OrderIndex).ToList(),
                    AttemptsUsed = 0,
                    AttemptsRemaining = exercise.MaxAttempts,
                    BestScore = null,
                    PreviousResults = new List<ExerciseResult>()
                };

                return View(viewModel);
            }

            // Regular student flow
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            // Kiểm tra: Chỉ cho phép làm bài tập của lớp hiện tại
            if (exercise.Lesson.Grade != student.Grade)
            {
                TempData["ErrorMessage"] = $"Bạn chỉ có thể làm bài tập của lớp {student.Grade}.";
                return RedirectToAction("Lessons");
            }

            var canAttempt = await _exerciseService.CanAttemptExerciseAsync(student.StudentId, id);
            if (!canAttempt)
            {
                TempData["ErrorMessage"] = "Bạn đã hết số lần làm bài cho bài tập này.";
                return RedirectToAction("LessonDetail", new { id = exercise.LessonId });
            }

            var results = await _exerciseService.GetStudentResultsAsync(student.StudentId, id);

            var studentViewModel = new ExerciseDetailViewModel
            {
                Exercise = exercise,
                Questions = exercise.Questions.OrderBy(q => q.OrderIndex).ToList(),
                AttemptsUsed = results.Count,
                AttemptsRemaining = exercise.MaxAttempts - results.Count,
                BestScore = results.Any() ? results.Max(r => r.Score) : null,
                PreviousResults = results.OrderByDescending(r => r.CompletedDate).Take(5).ToList()
            };

            return View(studentViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitExercise([FromBody] SubmitExerciseViewModel model)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Json(new { success = false, message = "Không tìm thấy thông tin học sinh" });

            try
            {
                var result = await _exerciseService.SubmitExerciseAsync(
                    student.StudentId,
                    model.ExerciseId,
                    model.Answers,
                    model.TimeSpent);

                // Save essay answers if any
                if (model.EssayAnswers != null && model.EssayAnswers.Any())
                {
                    foreach (var essayAnswer in model.EssayAnswers)
                    {
                        var essay = new EssayAnswer
                        {
                            ExerciseResultId = result.ResultId,
                            QuestionId = essayAnswer.QuestionId,
                            AnswerText = essayAnswer.AnswerText,
                            ImageUrl = essayAnswer.ImageUrl,
                            SubmittedDate = DateTime.Now
                        };
                        _context.EssayAnswers.Add(essay);
                    }

                    await _context.SaveChangesAsync();
                }

                var exercise = await _exerciseService.GetExerciseByIdAsync(model.ExerciseId);
                if (exercise != null)
                {
                    // Check if exercise has essay questions
                    var hasEssayQuestions = exercise.Questions.Any(q => q.Type == QuestionType.Essay);

                    // Tạo thông báo cho học sinh
                    if (hasEssayQuestions)
                    {
                        // Thông báo chờ chấm điểm tự luận
                        await _notificationService.CreateNotificationAsync(
                            student.StudentId,
                            "⏳ Bài tập đang chờ chấm",
                            $"Bạn đã nộp bài tập '{exercise.Title}'. Bài tập có phần tự luận đang chờ giáo viên chấm điểm. Bạn sẽ nhận được thông báo khi có kết quả! 📝",
                            NotificationType.System,
                            $"/Student/LessonDetail/{exercise.LessonId}"
                        );
                    }
                    else if (result.IsPassed)
                    {
                        // Cập nhật trạng thái hoàn thành bài học
                        await _progressService.MarkLessonCompletedAsync(student.StudentId, exercise.LessonId);

                        // Tạo notification khi hoàn thành bài tập
                        var lesson = await _context.Lessons.FindAsync(exercise.LessonId);
                        await _notificationService.CreateNotificationAsync(
                            student.StudentId,
                            "🎉 Hoàn thành bài tập!",
                            $"Chúc mừng! Bạn đã hoàn thành bài tập '{exercise.Title}' của bài học '{lesson?.Title}' với điểm số {result.Score:F1}/10. Tiếp tục phát huy nhé! 💪",
                            NotificationType.Achievement,
                            $"/Student/LessonDetail/{exercise.LessonId}"
                        );
                    }
                    else
                    {
                        // Thông báo động viên khi chưa đạt
                        await _notificationService.CreateNotificationAsync(
                            student.StudentId,
                            "💪 Hãy cố gắng thêm!",
                            $"Bạn đã hoàn thành bài tập '{exercise.Title}' với điểm {result.Score:F1}/10. Chưa đạt yêu cầu, nhưng đừng nản chí! Bạn còn {exercise.MaxAttempts - result.AttemptNumber} lần thử. Ôn lại bài và thử lại nhé! 📚",
                            NotificationType.LearningReminder,
                            $"/Student/Exercise/{exercise.ExerciseId}"
                        );
                    }

                    // Kiểm tra và tự động mở khóa bài tiếp (chỉ nếu không có essay hoặc đã pass)
                    if (!hasEssayQuestions && result.IsPassed)
                    {
                        await _progressService.CheckAndUnlockNextLessonAsync(student.StudentId, exercise.LessonId);
                    }
                }

                var hasEssayQuestionsInExercise = exercise != null && exercise.Questions.Any(q => q.Type == QuestionType.Essay);
                
                var resultViewModel = new ExerciseResultViewModel
                {
                    Score = result.Score,
                    CorrectAnswers = result.CorrectAnswers,
                    TotalQuestions = result.TotalQuestions,
                    IsPassed = result.IsPassed,
                    AttemptNumber = result.AttemptNumber,
                    AttemptsRemaining = exercise != null ? exercise.MaxAttempts - result.AttemptNumber : 0,
                    Message = hasEssayQuestionsInExercise
                        ? "Bài tập đã được nộp! Phần tự luận đang chờ giáo viên chấm điểm. Bạn sẽ nhận được thông báo khi có kết quả."
                        : (result.IsPassed
                            ? "Chúc mừng! Bạn đã hoàn thành bài tập!"
                            : "Chưa đạt. Hãy cố gắng thêm nhé!")
                };

                return Json(new { success = true, result = resultViewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Progress(int? grade)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            var progressList = await _progressService.GetStudentProgressAsync(student.StudentId);

            // Filter theo grade nếu có
            if (grade.HasValue)
            {
                progressList = progressList.Where(p => p.Lesson.Grade == grade.Value).ToList();
            }

            var viewModel = progressList.Select(p => new LessonProgressViewModel
            {
                LessonId = p.LessonId,
                Title = p.Lesson.Title,
                Topic = p.Lesson.Topic,
                Status = p.Status,
                CompletionPercentage = p.CompletionPercentage,
                CompletedDate = p.CompletedDate,
                TotalExercises = p.Lesson.Exercises.Count
            }).ToList();

            ViewBag.SelectedGrade = grade;
            return View(viewModel);
        }

        public async Task<IActionResult> Ranking(int? grade)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            var selectedGrade = grade; // Cho phép null = Tất cả

            // Lấy danh sách students từ DB trước
            var query = _context.Students.Where(s => s.IsActive);

            // Filter theo grade nếu có
            if (selectedGrade.HasValue)
            {
                query = query.Where(s => s.Grade == selectedGrade.Value);
            }

            var students = await query
                .OrderByDescending(s => s.TotalPoints)
                .ThenBy(s => s.StudentId) // Thêm sắp xếp phụ để ổn định thứ tự
                .Select(s => new
                {
                    s.StudentId,
                    s.StudentCode,
                    s.FullName,
                    s.Grade,
                    s.TotalPoints,
                    s.AvatarUrl
                })
                .ToListAsync();

            // Tính rank ở phía client
            var rankings = students.Select((s, index) => new RankingViewModel
            {
                Rank = index + 1,
                StudentCode = s.StudentCode,
                FullName = s.FullName,
                Grade = s.Grade,
                TotalPoints = s.TotalPoints,
                AvatarUrl = s.AvatarUrl,
                IsCurrentUser = s.StudentId == student.StudentId
            }).ToList();

            ViewBag.SelectedGrade = selectedGrade;
            ViewBag.StudentGrade = student.Grade;

            return View(rankings);
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationRead([FromBody] MarkNotificationRequest request)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Json(new { success = false });

            var result = await _notificationService.MarkAsReadAsync(request.NotificationId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return Json(new { success = false });

            var result = await _notificationService.MarkAllAsReadAsync(student.StudentId);
            return Json(new { success = result });
        }

        public async Task<IActionResult> Notifications()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            var notifications = await _notificationService.GetStudentNotificationsAsync(student.StudentId);
            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Admin không có trang Profile
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            var viewModel = new StudentProfileViewModel
            {
                FullName = student.FullName,
                Grade = student.Grade,
                DateOfBirth = student.DateOfBirth,
                ParentEmail = student.ParentEmail,
                AvatarUrl = student.AvatarUrl,
                StudentCode = student.StudentCode,
                TotalPoints = student.TotalPoints,
                BadgesEarned = student.BadgesEarned,
                EnrollmentDate = student.EnrollmentDate
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(StudentProfileViewModel model)
        {
            // Admin không có trang Profile
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            // Check if grade has changed
            var oldGrade = student.Grade;
            var gradeChanged = oldGrade != model.Grade;

            // Update student info
            student.FullName = model.FullName;
            student.Grade = model.Grade;
            student.DateOfBirth = model.DateOfBirth;
            student.ParentEmail = model.ParentEmail;
            student.AvatarUrl = model.AvatarUrl;

            // Also update the User FullName
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.FullName = model.FullName;
                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();

            // If grade changed, unlock lessons for new grade
            if (gradeChanged)
            {
                await _progressService.CheckAndUnlockAllEligibleLessonsAsync(student.StudentId, model.Grade);
                TempData["SuccessMessage"] =
                    $"Đã cập nhật hồ sơ và chuyển sang lớp {model.Grade}. Bạn có thể bắt đầu học các bài học mới!";
            }
            else
            {
                TempData["SuccessMessage"] = "Đã cập nhật hồ sơ cá nhân thành công!";
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Không có file được chọn" });

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "essays");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/uploads/essays/{uniqueFileName}";
                return Json(new { success = true, imageUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class MarkNotificationRequest
    {
        public int NotificationId { get; set; }
    }
}