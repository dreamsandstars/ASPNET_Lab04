using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AspNetWeek4.Mvc.Data;
using AspNetWeek4.Mvc.Models;
using AspNetWeek4.Mvc.Repositories;
using AspNetWeek4.Mvc.ViewModels;

namespace AspNetWeek4.Mvc.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly AppDbContext _context;

    public EnrollmentService(IEnrollmentRepository enrollmentRepository, AppDbContext context)
    {
        _enrollmentRepository = enrollmentRepository;
        _context = context;
    }

    public async Task CreateEnrollmentAsync(EnrollmentCreateViewModel model)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Find or create Student
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == model.StudentEmail);
            if (student == null)
            {
                student = new Student
                {
                    Name = model.StudentName,
                    Email = model.StudentEmail
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }

            // Find Course and verify available seats
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == model.CourseId);
            if (course == null)
            {
                throw new Exception("Selected course not found.");
            }

            if (course.AvailableSeats < model.Quantity)
            {
                throw new Exception($"Not enough seats available. Remaining seats: {course.AvailableSeats}, requested: {model.Quantity}");
            }

            // Create Enrollment
            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CreatedAt = DateTime.Now,
                TotalAmount = course.Price * model.Quantity
            };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Create EnrollmentDetail
            var detail = new EnrollmentDetail
            {
                EnrollmentId = enrollment.Id,
                CourseId = course.Id,
                Quantity = model.Quantity,
                UnitPrice = course.Price
            };
            _context.EnrollmentDetails.Add(detail);

            // Deduct available seats
            course.AvailableSeats -= model.Quantity;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CreateBulkEnrollmentAsync(BulkEnrollmentViewModel model)
    {
        if (model.Items == null || model.Items.Count == 0)
        {
            throw new Exception("Registration cart is empty.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == model.StudentEmail);
            if (student == null)
            {
                student = new Student
                {
                    Name = model.StudentName,
                    Email = model.StudentEmail
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = 0
            };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            decimal totalAmount = 0;

            foreach (var item in model.Items)
            {
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == item.CourseId);
                if (course == null)
                {
                    throw new Exception($"Course '{item.CourseName}' not found.");
                }

                if (course.AvailableSeats < item.Quantity)
                {
                    throw new Exception($"OVERSOLD: Not enough seats for '{course.Name}'. Remaining: {course.AvailableSeats}, Requested: {item.Quantity}");
                }

                var detail = new EnrollmentDetail
                {
                    EnrollmentId = enrollment.Id,
                    CourseId = course.Id,
                    Quantity = item.Quantity,
                    UnitPrice = course.Price
                };
                _context.EnrollmentDetails.Add(detail);

                course.AvailableSeats -= item.Quantity;
                _context.Courses.Update(course);

                totalAmount += course.Price * item.Quantity;
            }

            enrollment.TotalAmount = totalAmount;
            _context.Enrollments.Update(enrollment);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public Task<List<Enrollment>> GetEnrollmentHistoryAsync()
    {
        return _enrollmentRepository.GetAllReadOnlyAsync();
    }

    public Task<int> GetEnrollmentCountAsync()
    {
        return _enrollmentRepository.GetCountAsync();
    }

    public Task<int> GetStudentCountAsync()
    {
        return _enrollmentRepository.GetStudentCountAsync();
    }
}
