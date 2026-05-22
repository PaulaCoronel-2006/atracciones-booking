using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microservicios.Atracciones.Booking.DataAccess.Entities;

namespace Microservicios.Atracciones.Booking.DataAccess.Configurations;

public class BookingStatusConfiguration : IEntityTypeConfiguration<BookingStatus>
{
    public void Configure(EntityTypeBuilder<BookingStatus> builder)
    {
        builder.ToTable("BookingStatus");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).HasMaxLength(20).IsRequired();
        builder.HasData(
            new BookingStatus { Id = 1, Name = "Pending" },
            new BookingStatus { Id = 2, Name = "Confirmed" },
            new BookingStatus { Id = 3, Name = "Completed" },
            new BookingStatus { Id = 4, Name = "Cancelled" },
            new BookingStatus { Id = 5, Name = "NoShow" }
        );
    }
}

public class ReviewCriteriaConfiguration : IEntityTypeConfiguration<ReviewCriteria>
{
    public void Configure(EntityTypeBuilder<ReviewCriteria> builder)
    {
        builder.ToTable("ReviewCriteria");
        builder.HasKey(rc => rc.Id);
        builder.Property(rc => rc.Name).HasMaxLength(50).IsRequired();
        builder.HasData(
            new ReviewCriteria { Id = 1, Name = "Guide" },
            new ReviewCriteria { Id = 2, Name = "Punctuality" },
            new ReviewCriteria { Id = 3, Name = "ValueForMoney" },
            new ReviewCriteria { Id = 4, Name = "Safety" },
            new ReviewCriteria { Id = 5, Name = "Cleanliness" },
            new ReviewCriteria { Id = 6, Name = "Organization" }
        );
    }
}

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.ToTable("AvailabilitySlot", t => t.HasCheckConstraint("CK_AvailSlot_Capacity",
            "capacity_available <= capacity_total AND capacity_total > 0 AND capacity_available >= 0"));
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.ProductId, s.SlotDate, s.StartTime }).IsUnique();

        builder.Property(s => s.IsActive).HasDefaultValue(true);
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(s => s.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog", t => t.HasCheckConstraint("CK_AuditLog_Action",
            "action IN ('INSERT','UPDATE','DELETE')"));
        builder.HasKey(a => a.Id);
        builder.Property(a => a.TableName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(10).IsRequired();
        builder.Property(a => a.ChangedBy).HasMaxLength(256);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(500);
        builder.Property(a => a.Endpoint).HasMaxLength(255);
        builder.Property(a => a.ChangedAt).HasDefaultValueSql("GETUTCDATE()");
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Review");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Title).HasMaxLength(150);
        builder.Property(r => r.OverallScore).HasPrecision(3, 2);
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.Property(r => r.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(r => r.Ratings)
               .WithOne(rt => rt.Review)
               .HasForeignKey(rt => rt.ReviewId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Media)
               .WithOne(m => m.Review)
               .HasForeignKey(m => m.ReviewId)
               .OnDelete(DeleteBehavior.Cascade);
               
        // Referencia lógica
        builder.Property(r => r.UserId).IsRequired();
    }
}

public class ReviewRatingConfiguration : IEntityTypeConfiguration<ReviewRating>
{
    public void Configure(EntityTypeBuilder<ReviewRating> builder)
    {
        builder.ToTable("ReviewRating");
        builder.HasKey(rr => rr.Id);
        builder.HasIndex(rr => new { rr.ReviewId, rr.CriteriaId }).IsUnique();

        builder.HasOne(rr => rr.Criteria)
               .WithMany(c => c.Ratings)
               .HasForeignKey(rr => rr.CriteriaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReviewMediaConfiguration : IEntityTypeConfiguration<ReviewMedia>
{
    public void Configure(EntityTypeBuilder<ReviewMedia> builder)
    {
        builder.ToTable("ReviewMedia");
        builder.HasKey(rm => rm.Id);
        builder.Property(rm => rm.Url).IsRequired();
        builder.Property(rm => rm.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
    }
}
