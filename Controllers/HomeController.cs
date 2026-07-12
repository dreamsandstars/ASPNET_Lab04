using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using AspNetWeek4.Mvc.Models;
using AspNetWeek4.Mvc.Services;

namespace AspNetWeek4.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;

    public HomeController(
        ILogger<HomeController> logger,
        ICourseService courseService,
        IEnrollmentService enrollmentService)
    {
        _logger = logger;
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var courses = await _courseService.GetCourseListAsync();
            var categories = await _courseService.GetCategoryListAsync();
            var studentCount = await _enrollmentService.GetStudentCountAsync();
            var enrollmentCount = await _enrollmentService.GetEnrollmentCountAsync();
            var history = await _enrollmentService.GetEnrollmentHistoryAsync();

            ViewBag.CourseCount = courses.Count;
            ViewBag.CategoryCount = categories.Count;
            ViewBag.StudentCount = studentCount;
            ViewBag.EnrollmentCount = enrollmentCount;
            ViewBag.LowSeatsCount = courses.Count(c => c.IsLowSeats);
            ViewBag.RecentEnrollments = history.OrderByDescending(e => e.CreatedAt).Take(2).ToList();
            ViewBag.IsDbConnected = true;
        }
        catch
        {
            ViewBag.CourseCount = 0;
            ViewBag.CategoryCount = 0;
            ViewBag.StudentCount = 0;
            ViewBag.EnrollmentCount = 0;
            ViewBag.LowSeatsCount = 0;
            ViewBag.RecentEnrollments = new List<Enrollment>();
            ViewBag.IsDbConnected = false;
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? id = null)
    {
        Console.WriteLine($"[DEBUG] Error action called with id = '{id}'");
        if (id == 404)
        {
            return View("NotFound");
        }
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
