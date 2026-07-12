using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetWeek4.Mvc.Models;

namespace AspNetWeek4.Mvc.Repositories;

public interface IEnrollmentRepository
{
    Task<List<Enrollment>> GetAllAsync();
    Task<List<Enrollment>> GetAllReadOnlyAsync();
    Task<Enrollment?> GetByIdAsync(int id);
    Task AddAsync(Enrollment enrollment);
    Task SaveChangesAsync();
    Task<int> GetCountAsync();
    Task<int> GetStudentCountAsync();
}
