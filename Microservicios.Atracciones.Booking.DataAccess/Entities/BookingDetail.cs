namespace Microservicios.Atracciones.Booking.DataAccess.Entities;

public class BookingDetail
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public Guid ProductOptionId { get; set; }   // Referencia lógica
    public Guid PriceTierId { get; set; }       // Referencia lógica
    
    // Snapshots (Copia de seguridad del catálogo al momento de la reserva)
    public string AttractionNameSnapshot { get; set; } = string.Empty;
    public string OptionNameSnapshot { get; set; } = string.Empty;
    public string TierNameSnapshot { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "USD";

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public short Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }      // Precio capturado al momento de reservar
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Booking Booking { get; set; } = null!;
}
