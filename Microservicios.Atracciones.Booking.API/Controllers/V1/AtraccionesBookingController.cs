using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microservicios.Atracciones.Booking.Business.DTOs.Booking;
using Microservicios.Atracciones.Booking.Business.Interfaces;
using System.Security.Claims;

namespace Microservicios.Atracciones.Booking.API.Controllers.V1;

/// <summary>
/// Contrato REST público para la gestión de Reservas y Cancelaciones.
/// Este controlador maneja la parte transaccional del sistema.
/// </summary>
[ApiController]
[Route("api/v1/booking")]
[Authorize] 
[Produces("application/json")]
public class AtraccionesBookingController : ControllerBase
{
    private readonly IBookingIntegrationService _bookingService;

    public AtraccionesBookingController(IBookingIntegrationService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Crea una nueva reserva bloqueando el inventario de cupos.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AtraccionBookingResponseDto>>> CrearReserva([FromBody] AtraccionBookingRequestDto request)
    {
        request.Normalize();

        var userId = GetUserId();
        var result = await _bookingService.CrearReservaAsync(request, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Cancela una reserva y libera los cupos en el inventario.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<bool>>> CancelarReserva(Guid id)
    {
        var userId = GetUserId();
        var result = await _bookingService.CancelarReservaAsync(id, userId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Lista el historial de reservas del usuario autenticado.
    /// </summary>
    [HttpGet("mis-reservas")]
    public async Task<ActionResult<ApiResponse<List<AtraccionBookingResponseDto>>>> ListarMisReservas()
    {
        var userId = GetUserId();
        var result = await _bookingService.ListarMisReservasAsync(userId);
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
