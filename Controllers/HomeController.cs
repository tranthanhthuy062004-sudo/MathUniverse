using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MathUniverse.Data;
using MathUniverse.Models;
using MathUniverse.Services;

namespace MathUniverse.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly ILessonService _lessonService;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context,
        ILessonService lessonService)
    {
        _logger = logger;
        _context = context;
        _lessonService = lessonService;
    }

    public async Task<IActionResult> Index()
    {
        // Get some sample lessons for guests
        var sampleLessons = await _context.Lessons
            .Where(l => l.IsPublished)
            .OrderBy(l => l.Grade)
            .ThenBy(l => l.OrderIndex)
            .Take(6)
            .ToListAsync();

        ViewBag.SampleLessons = sampleLessons;

        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public async Task<IActionResult> Browse(int? grade)
    {
        var lessons = grade.HasValue
            ? await _lessonService.GetLessonsByGradeAsync(grade.Value)
            : await _context.Lessons
                .Where(l => l.IsPublished)
                .OrderBy(l => l.Grade)
                .ThenBy(l => l.OrderIndex)
                .Include(l => l.Exercises)
                .ToListAsync();

        ViewBag.SelectedGrade = grade;
        return View(lessons);
    }

    public async Task<IActionResult> LessonPreview(int id)
    {
        var lesson = await _lessonService.GetLessonByIdAsync(id);
        if (lesson == null || !lesson.IsPublished) return NotFound();

        return View(lesson);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult ThemeDemo()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

