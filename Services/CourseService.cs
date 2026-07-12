using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using AspNetWeek4.Mvc.Models;
using AspNetWeek4.Mvc.Options;
using AspNetWeek4.Mvc.Repositories;
using AspNetWeek4.Mvc.ViewModels;

namespace AspNetWeek4.Mvc.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly AppSettings _settings;

    public CourseService(ICourseRepository courseRepository, IOptions<AppSettings> options)
    {
        _courseRepository = courseRepository;
        _settings = options.Value;
    }

    public async Task<List<CourseListItemViewModel>> GetCourseListAsync()
    {
        var courses = await _courseRepository.GetAllReadOnlyAsync();
        return courses.Select(MapToViewModel).ToList();
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        return await _courseRepository.GetByIdAsync(id);
    }

    public async Task<List<CourseCategoryListItemViewModel>> GetCategoryListAsync()
    {
        var categories = await _courseRepository.GetAllCategoriesReadOnlyAsync();
        return categories.Select(c => new CourseCategoryListItemViewModel
        {
            Id = c.Id,
            Name = c.Name,
            CourseCount = c.Courses.Count
        }).ToList();
    }

    public async Task<List<CourseListItemViewModel>> GetFilteredCoursesAsync(int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var courses = await _courseRepository.GetFilteredReadOnlyAsync(categoryId, minPrice, maxPrice);
        return courses.Select(MapToViewModel).ToList();
    }

    private CourseListItemViewModel MapToViewModel(Course c)
    {
        return new CourseListItemViewModel
        {
            Id = c.Id,
            CourseCode = c.CourseCode, // Map CourseCode
            Name = c.Name,
            Price = c.Price,
            AvailableSeats = c.AvailableSeats,
            CategoryName = c.CourseCategory != null ? c.CourseCategory.Name : "N/A",
            IsLowSeats = c.AvailableSeats <= _settings.LowSeatThreshold // Feature 1 threshold logic
        };
    }
}
