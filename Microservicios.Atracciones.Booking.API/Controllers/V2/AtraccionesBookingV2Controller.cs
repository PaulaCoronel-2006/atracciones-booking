using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microservicios.Atracciones.Booking.API.Filters;
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
    [TypeFilter(typeof(IdempotentFilter))]
    public async Task<ActionResult<ApiResponse<AtraccionBookingResponseDto>>> CrearReserva(
        [FromBody] AtraccionBookingRequestDto request)
    {
        request.Normalize();

        var userId = GetUserId();
        
        string? rawIdempotencyKey = Request.Headers["X-Idempotency-Key"];
        if (string.IsNullOrEmpty(rawIdempotencyKey))
        {
            rawIdempotencyKey = Request.Headers["Idempotency-Key"];
        }
        
        var idempotencyKey = Guid.Parse(rawIdempotencyKey!);

        var result = await _bookingService.CrearReservaAsync(request, userId, idempotencyKey);

        if (!result.Success)
            return BadRequest(result);

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
