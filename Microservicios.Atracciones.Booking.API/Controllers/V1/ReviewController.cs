using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microservicios.Atracciones.Booking.Business.DTOs.Review;
using Microservicios.Atracciones.Booking.Business.Interfaces;
using System.Security.Claims;
using Microservicios.Atracciones.Booking.DataAccess.Common;

namespace Microservicios.Atracciones.Booking.API.Controllers.V1;

[ApiController]
[Route("api/v1/review")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Crea una nueva reseña de una atracción que un usuario ha visitado exitosamente.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ReviewResponse>> CreateReview([FromBody] CreateReviewRequest request)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        bool isAdmin = User.IsInRole("Admin");
        var result = await _reviewService.CreateReviewAsync(currentUserId, isAdmin, request);
        return Ok(result);
    }

    /// <summary>
    /// Búsqueda para el panel administrativo. 
    /// Filtra automáticamente por Partner si el usuario no es Admin.
    /// </summary>
    [HttpGet("management")]
    [Authorize(Roles = "Admin,Partner")]
    public async Task<ActionResult<PagedResult<ReviewResponse>>> SearchManagement([FromQuery] ReviewSearchRequest request)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        bool isAdmin = User.IsInRole("Admin");
        
        var result = await _reviewService.SearchManagementAsync(request, currentUserId, isAdmin);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene reseñas públicas de una atracción específica.
    /// </summary>
    [HttpGet("attraction/{attractionId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ReviewResponse>>> GetByAttraction(
        Guid attractionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetByAttractionAsync(attractionId, page, pageSize);
        return Ok(result);
    }


    /// <summary>
    /// Elimina una reseña inapropiada (Solo Admin).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteReview(Guid id)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        var success = await _reviewService.DeleteReviewAsync(id, currentUserId, isAdmin: true);
        if (!success) return NotFound();
        return Ok(new { message = "Reseña eliminada con éxito." });
    }
}
