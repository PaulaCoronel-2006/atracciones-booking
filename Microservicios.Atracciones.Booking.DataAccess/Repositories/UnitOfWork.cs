using Microservicios.Atracciones.Booking.DataAccess.Context;
using Microservicios.Atracciones.Booking.DataAccess.Repositories.Interfaces;

namespace Microservicios.Atracciones.Booking.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AtraccionDbContext _context;

    public UnitOfWork(AtraccionDbContext context)
    {
        _context = context;
    }

    private IAvailabilitySlotRepository? _availabilitySlots;
    public IAvailabilitySlotRepository AvailabilitySlots => _availabilitySlots ??= new AvailabilitySlotRepository(_context);

    private IBookingRepository? _bookings;
    public IBookingRepository Bookings => _bookings ??= new BookingRepository(_context);

    private IBookingDetailRepository? _bookingDetails;
    public IBookingDetailRepository BookingDetails => _bookingDetails ??= new BookingDetailRepository(_context);

    private IReviewRepository? _reviews;
    public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);

    private IReviewRatingRepository? _reviewRatings;
    public IReviewRatingRepository ReviewRatings => _reviewRatings ??= new ReviewRatingRepository(_context);

    private IReviewCriteriaRepository? _reviewCriterias;
    public IReviewCriteriaRepository ReviewCriterias => _reviewCriterias ??= new ReviewCriteriaRepository(_context);

    private IReviewMediaRepository? _reviewMedias;
    public IReviewMediaRepository ReviewMedias => _reviewMedias ??= new ReviewMediaRepository(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
