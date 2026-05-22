using Mapster;
using Microservicios.Atracciones.Booking.DataAccess.Entities;
using Microservicios.Atracciones.Booking.DataManagement.Models;

namespace Microservicios.Atracciones.Booking.DataManagement.Mappers;

public class InventoryMapperConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AvailabilitySlot, AvailabilitySlotNode>();
    }
}
