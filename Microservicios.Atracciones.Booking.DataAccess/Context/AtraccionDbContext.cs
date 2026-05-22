using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microservicios.Atracciones.Booking.DataAccess.Common;
using Microservicios.Atracciones.Booking.DataAccess.Entities;
using System.Text.Json;

namespace Microservicios.Atracciones.Booking.DataAccess.Context;

public class AtraccionDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AtraccionDbContext(
        DbContextOptions<AtraccionDbContext> options,
        IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // ══════════════════════════════════════════════════
    // 1. INVENTARIO
    // ══════════════════════════════════════════════════
    public DbSet<AvailabilitySlot> AvailabilitySlots { get; set; }

    // ══════════════════════════════════════════════════
    // 2. RESERVAS
    // ══════════════════════════════════════════════════
    public DbSet<BookingStatus> BookingStatuses { get; set; }
    public DbSet<DataAccess.Entities.Booking> Bookings { get; set; }
    public DbSet<BookingDetail> BookingDetails { get; set; }

    // ══════════════════════════════════════════════════
    // 3. RESEÑAS
    // ══════════════════════════════════════════════════
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewCriteria> ReviewCriterias { get; set; }
    public DbSet<ReviewRating> ReviewRatings { get; set; }
    public DbSet<ReviewMedia> ReviewMedias { get; set; }

    // ══════════════════════════════════════════════════
    // 4. AUDITORÍA
    // ══════════════════════════════════════════════════
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtraccionDbContext).Assembly);

        var tableMapping = new Dictionary<string, string>
        {
            { nameof(AvailabilitySlot), "availability_slot" },
            { "Booking", "booking" },
            { nameof(BookingStatus), "booking_status" },
            { nameof(BookingDetail), "booking_detail" },
            { nameof(Review), "review" },
            { nameof(ReviewRating), "review_rating" },
            { nameof(ReviewCriteria), "review_criteria" },
            { nameof(ReviewMedia), "review_media" },
            { nameof(AuditLog), "audit_log" }
        };

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var entityName = entity.ClrType.Name;
            if (tableMapping.TryGetValue(entityName, out var tableName))
            {
                entity.SetTableName(tableName);
            }

            foreach (var property in entity.GetProperties())
            {
                var propName = ToSnakeCase(property.Name);
                property.SetColumnName(propName);
            }
        }
    }

    private string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return System.Text.RegularExpressions.Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        var auditEntries = BuildAuditEntries();
        var result = await base.SaveChangesAsync(cancellationToken);
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }
        return result;
    }

    private void StampAuditFields()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    break;
            }
        }
    }

    private List<AuditLog> BuildAuditEntries()
    {
        ChangeTracker.DetectChanges();
        var ipAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        var endpoint = _httpContextAccessor?.HttpContext?.Request?.Path.ToString();
        var userAgent = _httpContextAccessor?.HttpContext?.Request?.Headers["User-Agent"].ToString();
        var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "system";

        var entries = new List<AuditLog>();
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog) continue;
            if (entry.State is EntityState.Detached or EntityState.Unchanged) continue;

            var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            if (idProp?.CurrentValue is not Guid recordId) continue;

            var action = entry.State switch
            {
                EntityState.Added => "INSERT",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => null
            };

            if (action is null) continue;

            entries.Add(new AuditLog
            {
                TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                RecordId = recordId,
                Action = action,
                ChangedBy = currentUser,
                ChangedAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Endpoint = endpoint,
                OldValues = action is "UPDATE" or "DELETE" ? JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue)) : null,
                NewValues = action is "INSERT" or "UPDATE" ? JsonSerializer.Serialize(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)) : null
            });
        }
        return entries;
    }
}
