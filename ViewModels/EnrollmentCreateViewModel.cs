using System.ComponentModel.DataAnnotations;

namespace AspNetWeek4.Mvc.ViewModels;

public class EnrollmentCreateViewModel
{
    [Required(ErrorMessage = "Please enter the student name.")]
    [StringLength(100, ErrorMessage = "Student name cannot exceed 100 characters.")]
    public string StudentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter an email address.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(150, ErrorMessage = "Email address cannot exceed 150 characters.")]
    public string StudentEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a course.")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Please enter the registration quantity.")]
    [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10.")]
    public int Quantity { get; set; } = 1;
}
