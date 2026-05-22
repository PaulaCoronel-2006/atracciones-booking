namespace Microservicios.Atracciones.Booking.DataAccess.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IAvailabilitySlotRepository AvailabilitySlots { get; }
    IBookingRepository Bookings { get; }
    IBookingDetailRepository BookingDetails { get; }
    IReviewRepository Reviews { get; }
    IReviewRatingRepository ReviewRatings { get; }
    IReviewCriteriaRepository ReviewCriterias { get; }
    IReviewMediaRepository ReviewMedias { get; }

    Task<int> CompleteAsync();
}
