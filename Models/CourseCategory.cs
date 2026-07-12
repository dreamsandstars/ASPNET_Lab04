using System.Collections.Generic;

namespace AspNetWeek4.Mvc.Models;

public class CourseCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
