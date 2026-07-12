using System;

namespace AspNetWeek4.Mvc.Models;

public class AuditLog
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
    public string EntityId { get; set; } = string.Empty;
    public string Changes { get; set; } = string.Empty; // JSON or formatted text representation of changes
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
