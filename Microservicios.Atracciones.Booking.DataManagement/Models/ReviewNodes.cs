namespace Microservicios.Atracciones.Booking.DataManagement.Models;

public class ReviewNode
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public Guid AttractionId { get; set; }
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Virtual expansion
    public string ClientName { get; set; } = string.Empty;
    public string AttractionName { get; set; } = string.Empty;
    public List<ReviewRatingNode> Ratings { get; set; } = [];
}

public class ReviewRatingNode
{
    public string Criteria { get; set; } = string.Empty; // "punctuality", "guide", etc
    public byte Rating { get; set; }
}
