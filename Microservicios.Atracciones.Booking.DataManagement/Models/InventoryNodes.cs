namespace Microservicios.Atracciones.Booking.DataManagement.Models;

public class AvailabilitySlotNode
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public short CapacityAvailable { get; set; }
    public string? Notes { get; set; }
}
