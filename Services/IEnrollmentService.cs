using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetWeek4.Mvc.Models;
using AspNetWeek4.Mvc.ViewModels;

namespace AspNetWeek4.Mvc.Services;

public interface IEnrollmentService
{
    Task CreateEnrollmentAsync(EnrollmentCreateViewModel model);
    Task CreateBulkEnrollmentAsync(BulkEnrollmentViewModel model);
    Task<List<Enrollment>> GetEnrollmentHistoryAsync();
    Task<int> GetEnrollmentCountAsync();
    Task<int> GetStudentCountAsync();
}
