using System.Collections.Generic;

namespace AspNetWeek4.Mvc.ViewModels;

public class DataHealthViewModel
{
    public bool IsDatabaseOnline { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseProvider { get; set; } = string.Empty;
    public int CategoryCount { get; set; }
    public int CourseCount { get; set; }
    public int EnrollmentCount { get; set; }
    public int StudentCount { get; set; }
    public List<string> AppliedMigrations { get; set; } = new List<string>();
    public List<string> PendingMigrations { get; set; } = new List<string>();
}
