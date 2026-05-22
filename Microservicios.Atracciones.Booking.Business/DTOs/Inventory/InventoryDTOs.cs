namespace Microservicios.Atracciones.Booking.Business.DTOs.Inventory;

/// <summary>Slot de disponibilidad en el calendario.</summary>
public class AvailabilitySlotResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public short CapacityTotal { get; set; }
    public short CapacityAvailable { get; set; }
    public bool IsActive { get; set; }
    /// <summary>Indica si el slot tiene al menos una reserva activa (no cancelada). No se puede eliminar si es true.</summary>
    public bool HasBookings { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Petición de consulta de disponibilidad.</summary>
public class AvailabilityRequest
{
    public Guid AttractionId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(30);
}
