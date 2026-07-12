using System;
using Microsoft.EntityFrameworkCore;
using AspNetWeek4.Mvc.Models;

namespace AspNetWeek4.Mvc.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<EnrollmentDetail> EnrollmentDetails => Set<EnrollmentDetail>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public override int SaveChanges()
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = base.SaveChanges();
        OnAfterSaveChanges(auditEntries).GetAwaiter().GetResult();
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries);
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry);
            auditEntry.EntityName = entry.Entity.GetType().Name;
            auditEntries.Add(auditEntry);

            if (entry.State == EntityState.Added)
            {
                auditEntry.Action = "INSERT";
                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.IsPrimaryKey()) continue;
                    auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                auditEntry.Action = "DELETE";
                foreach (var prop in entry.Properties)
                {
                    auditEntry.OldValues[prop.Metadata.Name] = prop.OriginalValue;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                auditEntry.Action = "UPDATE";
                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified && !Equals(prop.OriginalValue, prop.CurrentValue))
                    {
                        auditEntry.OldValues[prop.Metadata.Name] = prop.OriginalValue;
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
            }
        }

        foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
        {
            AuditLogs.Add(auditEntry.ToAudit());
        }

        return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
    }

    private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return Task.CompletedTask;

        foreach (var auditEntry in auditEntries)
        {
            foreach (var prop in auditEntry.Entry.Properties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    auditEntry.EntityId = prop.CurrentValue?.ToString() ?? "";
                }
            }
            AuditLogs.Add(auditEntry.ToAudit());
        }

        return base.SaveChangesAsync();
    }

    private class AuditEntry
    {
        public AuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            Entry = entry;
        }

        public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; }
        public string EntityName { get; set; } = "";
        public string Action { get; set; } = "";
        public string EntityId { get; set; } = "";
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();
        public List<Microsoft.EntityFrameworkCore.Metadata.IProperty> TemporaryProperties { get; } = new();

        public bool HasTemporaryProperties => TemporaryProperties.Any() || Entry.Properties.Any(p => p.IsTemporary);

        public AuditLog ToAudit()
        {
            var audit = new AuditLog();
            audit.EntityName = EntityName;
            audit.Action = Action;
            if (string.IsNullOrEmpty(EntityId))
            {
                foreach (var prop in Entry.Properties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        EntityId = prop.CurrentValue?.ToString() ?? "";
                    }
                }
            }
            audit.EntityId = EntityId;
            audit.Timestamp = DateTime.UtcNow;

            var changes = new System.Text.StringBuilder();
            if (Action == "INSERT")
            {
                changes.Append("Created: ");
                foreach (var val in NewValues)
                {
                    changes.Append($"[{val.Key}: {val.Value}] ");
                }
            }
            else if (Action == "DELETE")
            {
                changes.Append("Deleted: ");
                foreach (var val in OldValues)
                {
                    changes.Append($"[{val.Key}: {val.Value}] ");
                }
            }
            else if (Action == "UPDATE")
            {
                changes.Append("Modified: ");
                foreach (var val in NewValues)
                {
                    var oldVal = OldValues.ContainsKey(val.Key) ? OldValues[val.Key] : "N/A";
                    changes.Append($"[{val.Key}: {oldVal} -> {val.Value}] ");
                }
            }

            audit.Changes = changes.ToString();
            return audit;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CourseCategory>(entity =>
        {
            entity.ToTable("CourseCategories");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CourseCode).IsRequired().HasMaxLength(20).HasDefaultValue(""); // Added in Feature 3
            entity.HasOne(p => p.CourseCategory)
                  .WithMany(c => c.Courses)
                  .HasForeignKey(p => p.CourseCategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(150);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollments");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(o => o.Student)
                  .WithMany(c => c.Enrollments)
                  .HasForeignKey(o => o.StudentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EnrollmentDetail>(entity =>
        {
            entity.ToTable("EnrollmentDetails");
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(oi => oi.Enrollment)
                  .WithMany(o => o.EnrollmentDetails)
                  .HasForeignKey(oi => oi.EnrollmentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(oi => oi.Course)
                  .WithMany()
                  .HasForeignKey(oi => oi.CourseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed initial data
        modelBuilder.Entity<CourseCategory>().HasData(
            new CourseCategory { Id = 1, Name = "Programming" },
            new CourseCategory { Id = 2, Name = "Design" }
        );

        modelBuilder.Entity<Course>().HasData(
            new Course { Id = 1, CourseCode = "CS101", Name = "Introduction to C#", Price = 1200000, AvailableSeats = 15, CourseCategoryId = 1 },
            new Course { Id = 2, CourseCode = "CS201", Name = "ASP.NET Core Web MVC", Price = 3500000, AvailableSeats = 3, CourseCategoryId = 1 },
            new Course { Id = 3, CourseCode = "DS101", Name = "UI/UX Design Fundamentals", Price = 2000000, AvailableSeats = 8, CourseCategoryId = 2 }
        );

        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, Name = "Nguyen Van A", Email = "vana@gmail.com" },
            new Student { Id = 2, Name = "Tran Thi B", Email = "thib@gmail.com" }
        );
    }
}
