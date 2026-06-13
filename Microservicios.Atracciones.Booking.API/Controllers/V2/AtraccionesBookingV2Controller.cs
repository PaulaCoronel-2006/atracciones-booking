using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microservicios.Atracciones.Booking.Business.DTOs.Booking;
using Microservicios.Atracciones.Booking.Business.Interfaces;
using Microservicios.Atracciones.Booking.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Microservicios.Atracciones.Booking.API.Controllers.V2;

/// <summary>
/// Contrato REST público para la gestión de Reservas V2 con Idempotencia.
/// </summary>
[ApiController]
[Route("v2/booking")]
[Authorize] 
[Produces("application/json")]
public class AtraccionesBookingV2Controller : ControllerBase
{
    private readonly IBookingIntegrationService _bookingService;
    private readonly IUnitOfWork _uow;

    public AtraccionesBookingV2Controller(
        IBookingIntegrationService bookingService,
        IUnitOfWork uow)
    {
        _bookingService = bookingService;
        _uow = uow;
    }

    /// <summary>
    /// Crea una nueva reserva bloqueando el inventario de cupos. Requiere cabecera de idempotencia.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AtraccionBookingResponseDto>>> CrearReserva(
        [FromBody] AtraccionBookingRequestDto request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKeyHeader)
    {
        if (string.IsNullOrEmpty(idempotencyKeyHeader))
        {
            return BadRequest(ApiResponse<AtraccionBookingResponseDto>.Fail("La cabecera 'Idempotency-Key' es obligatoria para este endpoint."));
        }

        if (!Guid.TryParse(idempotencyKeyHeader, out var idempotencyKey))
        {
            return BadRequest(ApiResponse<AtraccionBookingResponseDto>.Fail("El valor de la cabecera 'Idempotency-Key' debe ser un UUID válido."));
        }

        request.Normalize();

        var userId = GetUserId();
        
        // 1. Verificar idempotencia buscando por correlation_id
        var existingBooking = await _uow.Bookings.Query()
            .Include(b => b.AvailabilitySlot)
            .FirstOrDefaultAsync(b => b.CorrelationId == idempotencyKey && b.UserId == userId);

        if (existingBooking != null)
        {
            Response.Headers.Append("X-Cache-Lookup", "HIT");
            
            var responseDto = new AtraccionBookingResponseDto
            {
                BookingId = existingBooking.Id,
                PnrCode = existingBooking.PnrCode,
                Status = existingBooking.StatusId == 4 ? "Cancelled" : "Confirmed",
                TotalAmount = existingBooking.TotalAmount,
                Currency = existingBooking.CurrencyCode,
                ActivityDate = existingBooking.AvailabilitySlot.SlotDate.ToDateTime(existingBooking.AvailabilitySlot.StartTime),
                AttractionName = request.AttractionName ?? "Attraction"
            };

            return Ok(ApiResponse<AtraccionBookingResponseDto>.Ok(responseDto, "Reserva recuperada por idempotencia."));
        }

        // 2. Proceder a crear la reserva pasándole el correlation_id
        var result = await _bookingService.CrearReservaAsync(request, userId, idempotencyKey);

        if (!result.Success)
            return BadRequest(result);

        Response.Headers.Append("X-Cache-Lookup", "MISS");
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("sub")?.Value;
                       
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("Usuario no identificado en el token.");

        return Guid.Parse(userIdClaim);
    }
}
