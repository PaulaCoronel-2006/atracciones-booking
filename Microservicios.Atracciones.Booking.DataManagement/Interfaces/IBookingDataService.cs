using Microservicios.Atracciones.Booking.DataAccess.Common;
using Microservicios.Atracciones.Booking.DataManagement.Models;

namespace Microservicios.Atracciones.Booking.DataManagement.Interfaces;

public interface IBookingDataService
{
    // Crear la reserva (generará el PNR internamente)
    Task<BookingNode?> CreateBookingAsync(BookingNode bookingNode);
    Task<BookingNode?> GetByPnrAsync(string pnrCode);
    Task<PagedResult<BookingNode>> GetBookingsByUserAsync(Guid userId, QueryFilters filters);
    Task<PagedResult<BookingNode>> SearchBookingsAsync(BookingQueryFilters filters);
    Task<bool> UpdateBookingStatusAsync(Guid bookingId, short statusId, string? cancelReason = null);
    Task<BookingNode?> GetByIdAsync(Guid id);
}
