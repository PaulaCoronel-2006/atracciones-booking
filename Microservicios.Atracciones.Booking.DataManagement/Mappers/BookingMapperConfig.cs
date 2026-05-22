using Mapster;
using Microservicios.Atracciones.Booking.DataManagement.Models;
using BookingEntity = Microservicios.Atracciones.Booking.DataAccess.Entities.Booking;
using BookingDetailEntity = Microservicios.Atracciones.Booking.DataAccess.Entities.BookingDetail;

namespace Microservicios.Atracciones.Booking.DataManagement.Mappers;

public class BookingMapperConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BookingEntity, BookingNode>()
            .Map(dest => dest.StatusName, src => src.Status != null ? src.Status.Name : string.Empty)
            .Map(dest => dest.AttractionId, src => src.AttractionId)
            .Map(dest => dest.SlotDate, src => src.AvailabilitySlot != null ? src.AvailabilitySlot.SlotDate : default)
            .Map(dest => dest.SlotStartTime, src => src.AvailabilitySlot != null ? src.AvailabilitySlot.StartTime : default)
            .Map(dest => dest.Details, src => src.Details);

        config.NewConfig<BookingDetailEntity, BookingDetailNode>()
            .Map(dest => dest.PriceTierLabel, src => src.TierNameSnapshot)
            .Map(dest => dest.AttractionName, src => src.AttractionNameSnapshot)
            .Map(dest => dest.ProductTitle, src => src.OptionNameSnapshot);
            
        config.NewConfig<BookingNode, BookingEntity>()
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.AvailabilitySlot)
            .Map(dest => dest.Details, src => src.Details);
            
        config.NewConfig<BookingDetailNode, BookingDetailEntity>()
            .Map(dest => dest.TierNameSnapshot, src => src.PriceTierLabel)
            .Map(dest => dest.AttractionNameSnapshot, src => src.AttractionName)
            .Map(dest => dest.OptionNameSnapshot, src => src.ProductTitle)
            .Ignore(dest => dest.Booking)
            .Ignore(dest => dest.CreatedAt);
    }
}
