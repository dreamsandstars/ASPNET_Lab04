namespace AspNetWeek4.Mvc.ViewModels;

public class CourseListItemViewModel
{
    public int Id { get; set; }
    public string? CourseCode { get; set; } // Will be mapped after adding CourseCode property
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsLowSeats { get; set; } // Feature 1: Seat threshold warning flag
}
