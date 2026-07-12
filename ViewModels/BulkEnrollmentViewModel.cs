using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspNetWeek4.Mvc.ViewModels;

public class BulkEnrollmentViewModel
{
    [Required(ErrorMessage = "Student Name is required")]
    [StringLength(100, ErrorMessage = "Student Name cannot exceed 100 characters")]
    public string StudentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Student Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
    public string StudentEmail { get; set; } = string.Empty;

    public List<CartItemViewModel> Items { get; set; } = new();
}

public class CartItemViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
