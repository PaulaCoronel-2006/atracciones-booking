using Microservicios.Atracciones.Booking.Business.DTOs.Booking;

namespace Microservicios.Atracciones.Booking.Business.Interfaces;

/// <summary>
/// Servicio de integración con el sistema central de Booking.
/// Expone los endpoints del contrato REST público para transacciones de reserva.
/// </summary>
public interface IBookingIntegrationService
{
    /// <summary>
    /// Consulta la disponibilidad de una atracción agrupada por día.
    /// </summary>
    Task<ApiResponse<List<DisponibilidadDiariaDto>>> ObtenerDisponibilidadAsync(Guid attractionId, DateOnly? fecha = null);

    // ── Transacciones ─────────────────────────────────────

    /// <summary>
    /// Crea una nueva reserva bloqueando cupos en el inventario.
    /// </summary>
    Task<ApiResponse<AtraccionBookingResponseDto>> CrearReservaAsync(AtraccionBookingRequestDto request, Guid userId, Guid? correlationId = null);

    /// <summary>
    /// Cancela una reserva existente y libera los cupos.
    /// </summary>
    Task<ApiResponse<bool>> CancelarReservaAsync(Guid bookingId, Guid userId);

    /// <summary>
    /// Lista las reservas realizadas por un usuario.
    /// </summary>
    Task<ApiResponse<List<AtraccionBookingResponseDto>>> ListarMisReservasAsync(Guid userId);

    /// <summary>
    /// Genera slots masivamente en un rango de fechas.
    /// </summary>
    Task<ApiResponse<int>> GenerarSlotsMasivoAsync(GenerateSlotsRequestDto request);

    /// <summary>
    /// Elimina slots en lote para una opción de producto y rango de fechas.
    /// </summary>
    Task<ApiResponse<int>> EliminarSlotsEnLoteAsync(BulkDeleteSlotsRequestDto request);
}

