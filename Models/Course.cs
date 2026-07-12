using System.ComponentModel.DataAnnotations;

namespace AspNetWeek4.Mvc.Models;

public class Course
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string CourseCode { get; set; } = string.Empty; // Added in Feature 3

    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    public int CourseCategoryId { get; set; }
    public CourseCategory? CourseCategory { get; set; }
}
