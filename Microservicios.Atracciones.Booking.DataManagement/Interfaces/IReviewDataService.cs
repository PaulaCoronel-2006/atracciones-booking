using Microservicios.Atracciones.Booking.DataAccess.Common;
using Microservicios.Atracciones.Booking.DataManagement.Models;

namespace Microservicios.Atracciones.Booking.DataManagement.Interfaces;

public interface IReviewDataService
{
    Task<bool> AddReviewAsync(ReviewNode reviewNode);
    Task<PagedResult<ReviewNode>> GetReviewsByAttractionAsync(Guid attractionId, QueryFilters filters);
    Task<PagedResult<ReviewNode>> SearchReviewsAsync(ReviewQueryFilters filters);
    Task<decimal> GetAverageRatingByAttractionAsync(Guid attractionId);
    Task<bool> DeleteAsync(Guid reviewId);
}
