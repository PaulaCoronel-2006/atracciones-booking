using Microservicios.Atracciones.Booking.DataAccess.Context;
using Microservicios.Atracciones.Booking.DataAccess.Entities;
using Microservicios.Atracciones.Booking.DataAccess.Repositories.Interfaces;

namespace Microservicios.Atracciones.Booking.DataAccess.Repositories;

// ── Inventario ──────────────────────────────────────
public class AvailabilitySlotRepository : GenericRepository<AvailabilitySlot>, IAvailabilitySlotRepository 
{ public AvailabilitySlotRepository(AtraccionDbContext context) : base(context) { } }

// ── Reservas ────────────────────────────────────────
public class BookingRepository : GenericRepository<DataAccess.Entities.Booking>, IBookingRepository 
{ public BookingRepository(AtraccionDbContext context) : base(context) { } }

public class BookingDetailRepository : GenericRepository<BookingDetail>, IBookingDetailRepository 
{ public BookingDetailRepository(AtraccionDbContext context) : base(context) { } }

// ── Reseñas ─────────────────────────────────────────
public class ReviewRepository : GenericRepository<Review>, IReviewRepository 
{ public ReviewRepository(AtraccionDbContext context) : base(context) { } }

public class ReviewRatingRepository : GenericRepository<ReviewRating>, IReviewRatingRepository 
{ public ReviewRatingRepository(AtraccionDbContext context) : base(context) { } }

public class ReviewCriteriaRepository : GenericRepository<ReviewCriteria>, IReviewCriteriaRepository 
{ public ReviewCriteriaRepository(AtraccionDbContext context) : base(context) { } }

public class ReviewMediaRepository : GenericRepository<ReviewMedia>, IReviewMediaRepository 
{ public ReviewMediaRepository(AtraccionDbContext context) : base(context) { } }
