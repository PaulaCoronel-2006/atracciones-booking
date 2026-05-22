namespace Microservicios.Atracciones.Booking.DataManagement.Models;

public class BookingNode
{
    public Guid Id { get; set; }
    public string PnrCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid SlotId { get; set; }
    public short StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Información útil desnormalizada
    public string UserEmail { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public Guid AttractionId { get; set; }
    public string AttractionName { get; set; } = string.Empty;
    public string AttractionSlug { get; set; } = string.Empty;
    public string ProductTitle { get; set; } = string.Empty;
    public DateOnly SlotDate { get; set; }
    public TimeOnly SlotStartTime { get; set; }
    public int CancelPolicyHours { get; set; }
    
    public List<BookingDetailNode> Details { get; set; } = [];
}

public class BookingDetailNode
{
    public Guid PriceTierId { get; set; }
    public string PriceTierLabel { get; set; } = string.Empty;
    public string AttractionName { get; set; } = string.Empty;
    public string ProductTitle { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public short Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
