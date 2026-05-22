namespace Microservicios.Atracciones.Booking.DataAccess.Entities;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid AttractionId { get; set; }
    public decimal OverallScore { get; set; }           // 1.00 - 5.00
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string? Response { get; set; }               // Respuesta del operador
    public DateTime? RespondedAt { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsVerified { get; set; } = true;
    public short? LanguageId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Booking Booking { get; set; } = null!;
    public virtual ICollection<ReviewRating> Ratings { get; set; } = [];
    public virtual ICollection<ReviewMedia> Media { get; set; } = [];
}
