using Microservicios.Atracciones.Booking.DataManagement.Models;

namespace Microservicios.Atracciones.Booking.DataManagement.Interfaces;

public interface IInventoryDataService
{
    /// <summary>
    /// Consulta slots de disponibilidad para una atracción en un rango de fechas.
    /// </summary>
    Task<IEnumerable<AvailabilitySlotNode>> GetAvailabilityAsync(Guid attractionId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Obtiene un slot específico por su ID.
    /// </summary>
    Task<AvailabilitySlotNode?> GetSlotByIdAsync(Guid slotId);

    /// <summary>
    /// Decrementa la capacidad disponible de un slot. Admite valores negativos para incrementar.
    /// </summary>
    Task<bool> DecrementSlotCapacityAsync(Guid slotId, short quantity);
}
