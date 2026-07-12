using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetWeek4.Mvc.Models;
using AspNetWeek4.Mvc.ViewModels;

namespace AspNetWeek4.Mvc.Services;

public interface ICourseService
{
    Task<List<CourseListItemViewModel>> GetCourseListAsync();
    Task<Course?> GetCourseByIdAsync(int id);
    Task<List<CourseCategoryListItemViewModel>> GetCategoryListAsync();
    Task<List<CourseListItemViewModel>> GetFilteredCoursesAsync(int? categoryId, decimal? minPrice, decimal? maxPrice);
}
