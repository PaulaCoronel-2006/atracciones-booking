namespace Microservicios.Atracciones.Booking.DataAccess.Entities;

/// <summary>Lookup table de estados de reserva.</summary>
public class BookingStatus
{
    public short Id { get; set; }
    public string Name { get; set; } = string.Empty;   // Pending | Confirmed | Completed | Cancelled | NoShow
    public virtual ICollection<Booking> Bookings { get; set; } = [];
}
