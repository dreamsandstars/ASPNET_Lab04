using System;
using System.Collections.Generic;

namespace AspNetWeek4.Mvc.Models;

public class Enrollment
{
    public int Id { get; set; }
    public int? StudentId { get; set; }
    public Student? Student { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public decimal TotalAmount { get; set; }
    public ICollection<EnrollmentDetail> EnrollmentDetails { get; set; } = new List<EnrollmentDetail>();
}
