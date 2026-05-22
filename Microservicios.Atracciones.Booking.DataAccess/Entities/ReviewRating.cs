namespace Microservicios.Atracciones.Booking.DataAccess.Entities;

public class ReviewRating
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReviewId { get; set; }
    public short CriteriaId { get; set; }
    public short Score { get; set; }        // 1 - 5

    public virtual Review Review { get; set; } = null!;
    public virtual ReviewCriteria Criteria { get; set; } = null!;
}
