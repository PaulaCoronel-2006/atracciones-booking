using Microservicios.Atracciones.Booking.DataAccess.Entities;

namespace Microservicios.Atracciones.Booking.DataAccess.Repositories.Interfaces;

// ── Inventario ──────────────────────────────────────
public interface IAvailabilitySlotRepository : IGenericRepository<AvailabilitySlot> { }

// ── Reservas ────────────────────────────────────────
public interface IBookingRepository : IGenericRepository<DataAccess.Entities.Booking> { }
public interface IBookingDetailRepository : IGenericRepository<BookingDetail> { }

// ── Reseñas ─────────────────────────────────────────
public interface IReviewRepository : IGenericRepository<Review> { }
public interface IReviewRatingRepository : IGenericRepository<ReviewRating> { }
public interface IReviewCriteriaRepository : IGenericRepository<ReviewCriteria> { }
public interface IReviewMediaRepository : IGenericRepository<ReviewMedia> { }
