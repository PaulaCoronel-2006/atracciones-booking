namespace Microservicios.Atracciones.Booking.DataAccess.Entities;

public class ReviewMedia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReviewId { get; set; }
    public string Url { get; set; } = string.Empty;
    public short SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Review Review { get; set; } = null!;
}
