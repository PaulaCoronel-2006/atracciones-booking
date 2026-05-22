namespace Microservicios.Atracciones.Booking.DataAccess.Entities;

/// <summary>Catálogo de criterios de calificación de reseñas.</summary>
public class ReviewCriteria
{
    public short Id { get; set; }
    public string Name { get; set; } = string.Empty;   // Guide | Punctuality | ValueForMoney | Safety | Cleanliness | Organization
    public virtual ICollection<ReviewRating> Ratings { get; set; } = [];
}
