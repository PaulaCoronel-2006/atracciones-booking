using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microservicios.Atracciones.Booking.Business.DTOs.Booking;
using Microservicios.Atracciones.Booking.Business.Interfaces;
using Microservicios.Atracciones.Booking.DataAccess.Common;

namespace Microservicios.Atracciones.Booking.API.Controllers.V1;

[ApiController]
[Route("api/v1/admin-booking")]
[Authorize] // Todas las acciones de reserva requieren un usuario autenticado por defecto
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Crea una nueva reserva (Checkout).
    /// Disminuye automáticamente el inventario (availability slot).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Client,Admin,Partner")]
    public async Task<ActionResult<BookingConfirmationResponse>> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var response = await _bookingService.CreateBookingAsync(currentUserId, request);
        return Ok(response);
    }

    /// <summary>
    /// Búsqueda para el panel administrativo. 
    /// Filtra automáticamente por Partner si el usuario no es Admin.
    /// </summary>
    [HttpGet("management")]
    [Authorize(Roles = "Admin,Partner")]
    public async Task<ActionResult<PagedResult<BookingSummaryResponse>>> SearchManagement([FromQuery] BookingSearchRequest request)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        bool isAdmin = User.IsInRole("Admin");
        
        var result = await _bookingService.SearchManagementAsync(request, currentUserId, isAdmin);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle de una reserva específica de forma pública con su código PNR.
    /// Útil para vistas de "Detalle de tu orden" sin estar logueado obligatoriamente en ese momento.
    /// </summary>
    [HttpGet("{pnr}")]
    [AllowAnonymous]
    public async Task<ActionResult<BookingDetailResponse>> GetBookingByPnr(string pnr)
    {
        var response = await _bookingService.GetByPnrAsync(pnr);
        return Ok(response);
    }

    /// <summary>
    /// Cancela una reserva existente devolviendo el espacio al inventario si aplica.
    /// </summary>
    [HttpPost("cancel")]
    [Authorize(Roles = "Client,Admin")]
    public async Task<ActionResult> CancelBooking([FromBody] CancelBookingRequest request)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        bool isAdmin = User.IsInRole("Admin");
        await _bookingService.CancelBookingAsync(currentUserId, isAdmin, request);
        return Ok(new { Message = "Reserva cancelada correctamente." });
    }

    /// <summary>
    /// Historial de reservas del cliente autenticado.
    /// </summary>
    [HttpGet("user/history")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<PagedResult<BookingSummaryResponse>>> GetMyBookings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var result = await _bookingService.GetUserHistoryAsync(currentUserId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle completo de una reserva por su ID (cliente dueño o Admin).
    /// </summary>
    [HttpGet("detail/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<BookingDetailResponse>> GetById(Guid id)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        bool isAdmin = User.IsInRole("Admin");
        var result = await _bookingService.GetByIdAsync(id, currentUserId, isAdmin);
        return Ok(result);
    }
}
