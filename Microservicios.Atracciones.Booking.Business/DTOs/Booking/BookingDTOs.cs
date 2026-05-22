namespace Microservicios.Atracciones.Booking.Business.DTOs.Booking;

// ─── REQUEST ─────────────────────────────────────────────────────────────────

/// <summary>Petición de creación de reserva.</summary>
public class CreateBookingRequest
{
    public Guid SlotId { get; set; }
    public Guid AttractionId { get; set; }          // Identificador lógico de la atracción
    public string AttractionName { get; set; } = string.Empty; // Snapshot del nombre
    public string ProductTitle { get; set; } = string.Empty;   // Snapshot del nombre de la opción/producto
    public short? LanguageId { get; set; }
    public string? Notes { get; set; }

    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    
    /// <summary>Lista de pasajeros y su precio correspondiente.</summary>
    public List<BookingPassengerRequest> Passengers { get; set; } = [];
}

/// <summary>Datos de un pasajero individual dentro de la reserva.</summary>
public class BookingPassengerRequest
{
    public Guid PriceTierId { get; set; }   // Identificador lógico de la categoría de precio
    public string PriceTierLabel { get; set; } = string.Empty; // Snapshot del nombre (ej. Adulto)
    public decimal UnitPrice { get; set; }             // Snapshot del precio en el momento de reserva
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public short Quantity { get; set; } = 1;
}

/// <summary>Petición de cancelación de reserva.</summary>
public class CancelBookingRequest
{
    public string PnrCode { get; set; } = string.Empty;
    public string? CancelReason { get; set; }
}

/// <summary>Filtros para búsqueda administrativa de reservas.</summary>
public class BookingSearchRequest
{
    public string? SearchTerm { get; set; } // PNR
    public short? StatusId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// ─── RESPONSE ────────────────────────────────────────────────────────────────

/// <summary>Respuesta de confirmación de reserva.</summary>
public class BookingConfirmationResponse
{
    public Guid Id { get; set; }
    public string PnrCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string AttractionName { get; set; } = string.Empty;
    public DateOnly SlotDate { get; set; }
    public TimeOnly SlotStartTime { get; set; }
    public int TotalPassengers { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Resumen de una reserva para listados de historial.</summary>
public class BookingSummaryResponse
{
    public Guid Id { get; set; }
    public string PnrCode { get; set; } = string.Empty;
    public string AttractionName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public decimal totalAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateOnly SlotDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public TimeOnly SlotStartTime { get; set; }
    public int TotalPassengers { get; set; }
    public short StatusId { get; set; }
    public bool CanReview { get; set; }
    public List<BookingTicketSummary> Tickets { get; set; } = [];

    // Cambiado de totalAmount (minúscula) a TotalAmount (mayúscula) para consistencia si es necesario
    public decimal TotalAmount { get => totalAmount; set => totalAmount = value; }
}

public class BookingTicketSummary
{
    public string CategoryName { get; set; } = string.Empty;
    public short Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>Detalle completo de una reserva.</summary>
public class BookingDetailResponse
{
    public Guid Id { get; set; }
    public string PnrCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public string AttractionName { get; set; } = string.Empty;
    public DateOnly SlotDate { get; set; }
    public TimeOnly SlotStartTime { get; set; }
    public bool CanReview { get; set; }

    public List<PassengerDetailResponse> Passengers { get; set; } = [];
}

public class PassengerDetailResponse
{
    public string FullName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string PriceTierLabel { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public short Quantity { get; set; }
}
