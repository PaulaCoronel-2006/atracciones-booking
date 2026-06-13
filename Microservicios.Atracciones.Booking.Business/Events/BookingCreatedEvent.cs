namespace Microservicios.Atracciones.Common.Events;

public record BookingCreatedEvent
{
    public Guid BookingId { get; init; }
    public Guid UserId { get; init; }
    public string PnrCode { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string CurrencyCode { get; init; } = "USD";
    public DateTime CreatedAt { get; init; }
    public string AttractionName { get; init; } = string.Empty;
    
    // Información de facturación requerida por Billing
    public string CustomerName { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
}
