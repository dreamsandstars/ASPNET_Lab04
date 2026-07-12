using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AspNetWeek4.Mvc.Services;

namespace AspNetWeek4.Mvc.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _courseService.GetCourseListAsync();
        return View(courses);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    public async Task<IActionResult> Filter(int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var filteredCourses = await _courseService.GetFilteredCoursesAsync(categoryId, minPrice, maxPrice);
        var categories = await _courseService.GetCategoryListAsync();

        ViewBag.Categories = categories;
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;

        return View(filteredCourses);
    }
}
