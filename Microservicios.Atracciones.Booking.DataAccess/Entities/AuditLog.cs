using System.ComponentModel.DataAnnotations.Schema; // Importante

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TableName { get; set; } = string.Empty;
    public Guid RecordId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [Column("old_values", TypeName = "jsonb")] // <--- Fuerza el tipo jsonb aquí
    public string? OldValues { get; set; }

    [Column("new_values", TypeName = "jsonb")] // <--- Fuerza el tipo jsonb aquí
    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Endpoint { get; set; }
}
