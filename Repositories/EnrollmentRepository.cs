using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AspNetWeek4.Mvc.Data;
using AspNetWeek4.Mvc.Models;

namespace AspNetWeek4.Mvc.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly AppDbContext _context;

    public EnrollmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Enrollment>> GetAllAsync()
        => _context.Enrollments
            .Include(o => o.EnrollmentDetails)
                .ThenInclude(oi => oi.Course)
            .Include(o => o.Student)
            .ToListAsync();

    public Task<List<Enrollment>> GetAllReadOnlyAsync()
        => _context.Enrollments
            .Include(o => o.EnrollmentDetails)
                .ThenInclude(oi => oi.Course)
            .Include(o => o.Student)
            .AsNoTracking()
            .ToListAsync();

    public Task<Enrollment?> GetByIdAsync(int id)
        => _context.Enrollments
            .Include(o => o.EnrollmentDetails)
                .ThenInclude(oi => oi.Course)
            .Include(o => o.Student)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task AddAsync(Enrollment enrollment)
        => await _context.Enrollments.AddAsync(enrollment);

    public Task SaveChangesAsync()
        => _context.SaveChangesAsync();

    public Task<int> GetCountAsync()
        => _context.Enrollments.CountAsync();

    public Task<int> GetStudentCountAsync()
        => _context.Students.CountAsync();
}
