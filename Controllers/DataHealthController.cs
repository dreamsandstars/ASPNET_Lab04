using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetWeek4.Mvc.Data;
using AspNetWeek4.Mvc.Models;
using AspNetWeek4.Mvc.ViewModels;

namespace AspNetWeek4.Mvc.Controllers;

public class DataHealthController : Controller
{
    private readonly AppDbContext _context;

    public DataHealthController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DataHealthViewModel();

        try
        {
            model.IsDatabaseOnline = await _context.Database.CanConnectAsync();
            model.ConnectionString = _context.Database.GetDbConnection().ConnectionString;
            model.DatabaseProvider = _context.Database.ProviderName ?? "N/A";

            if (model.IsDatabaseOnline)
            {
                model.CategoryCount = await _context.CourseCategories.CountAsync();
                model.CourseCount = await _context.Courses.CountAsync();
                model.EnrollmentCount = await _context.Enrollments.CountAsync();
                model.StudentCount = await _context.Students.CountAsync();

                model.AppliedMigrations = (await _context.Database.GetAppliedMigrationsAsync()).ToList();
                model.PendingMigrations = (await _context.Database.GetPendingMigrationsAsync()).ToList();
            }
        }
        catch (Exception ex)
        {
            model.IsDatabaseOnline = false;
            model.ConnectionString = $"Connection error: {ex.Message}";
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetDatabase()
    {
        try
        {
            // Drop database if exists and run migrations (re-seeds initial data automatically)
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.MigrateAsync();

            TempData["SuccessMessage"] = "Database successfully reset and re-seeded to original state!";

            // Record manual audit log
            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Database",
                Action = "RESET",
                EntityId = "ALL",
                Changes = "Database was wiped out and initial seeding applied.",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to reset database: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> SimulateTraffic()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var courses = await _context.Courses.ToListAsync();
            if (!courses.Any())
            {
                TempData["ErrorMessage"] = "No courses available to simulate traffic.";
                return RedirectToAction(nameof(Index));
            }

            var random = new Random();
            var course = courses[random.Next(courses.Count)];

            // Create random student
            var studentId = random.Next(1000, 9999);
            var student = new Student
            {
                Name = $"Simulated Student #{studentId}",
                Email = $"simulated.{studentId}@trainingcenter.edu.vn"
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Attempt to register 1 seat
            if (course.AvailableSeats < 1)
            {
                throw new InvalidOperationException($"OVERSOLD: Course '{course.Name}' has 0 available seats left!");
            }

            course.AvailableSeats -= 1;
            _context.Courses.Update(course);

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                TotalAmount = course.Price,
                CreatedAt = DateTime.UtcNow
            };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            var detail = new EnrollmentDetail
            {
                EnrollmentId = enrollment.Id,
                CourseId = course.Id,
                Quantity = 1,
                UnitPrice = course.Price
            };
            _context.EnrollmentDetails.Add(detail);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            TempData["SuccessMessage"] = $"Simulated enrollment of {student.Name} to course '{course.Name}' succeeded!";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["ErrorMessage"] = $"Traffic Simulation failed & rolled back! Reason: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> AuditLogs()
    {
        var logs = await _context.AuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(100)
            .ToListAsync();
        return View(logs);
    }
}
