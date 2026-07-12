using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AspNetWeek4.Mvc.Services;
using AspNetWeek4.Mvc.ViewModels;

namespace AspNetWeek4.Mvc.Controllers;

public class EnrollmentsController : Controller
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICourseService _courseService;

    public EnrollmentsController(IEnrollmentService enrollmentService, ICourseService courseService)
    {
        _enrollmentService = enrollmentService;
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Courses = await _courseService.GetCourseListAsync();
        return View(new EnrollmentCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EnrollmentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Courses = await _courseService.GetCourseListAsync();
            return View(model);
        }

        try
        {
            await _enrollmentService.CreateEnrollmentAsync(model);
            TempData["SuccessMessage"] = "Course enrolled successfully!";
            return RedirectToAction(nameof(Success));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Enrollment error: {ex.Message}");
            ViewBag.Courses = await _courseService.GetCourseListAsync();
            return View(model);
        }
    }

    public IActionResult Success()
    {
        return View();
    }

    public async Task<IActionResult> History()
    {
        var history = await _enrollmentService.GetEnrollmentHistoryAsync();
        return View(history);
    }

    [HttpGet]
    public IActionResult Cart()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CheckoutCart([FromBody] BulkEnrollmentViewModel model)
    {
        if (model == null)
        {
            return Json(new { success = false, message = "Invalid data payload." });
        }

        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = errors });
        }

        try
        {
            await _enrollmentService.CreateBulkEnrollmentAsync(model);
            TempData["SuccessMessage"] = "Bulk registration checkout completed successfully!";
            return Json(new { success = true, redirectUrl = Url.Action(nameof(Success)) });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
