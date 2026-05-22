using FluentValidation;
using Microservicios.Atracciones.Booking.Business.DTOs.Review;
using Microservicios.Atracciones.Booking.Business.Exceptions;
using Microservicios.Atracciones.Booking.Business.Interfaces;
using Microservicios.Atracciones.Booking.DataAccess.Common;
using Microservicios.Atracciones.Booking.DataManagement.Interfaces;
using Microservicios.Atracciones.Booking.DataManagement.Models;

namespace Microservicios.Atracciones.Booking.Business.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewDataService _reviewData;
    private readonly IBookingDataService _bookingData;
    private readonly IValidator<CreateReviewRequest> _validator;

    public ReviewService(
        IReviewDataService reviewData,
        IBookingDataService bookingData,
        IValidator<CreateReviewRequest> validator)
    {
        _reviewData = reviewData;
        _bookingData = bookingData;
        _validator = validator;
    }

    public async Task<ReviewResponse> CreateReviewAsync(Guid userId, bool isAdmin, CreateReviewRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new Exceptions.ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var booking = await _bookingData.GetByPnrAsync(request.PnrCode.ToUpperInvariant());
        if (booking == null)
            throw new NotFoundException("Reserva", request.PnrCode);

        if (!isAdmin && booking.UserId != userId)
            throw new UnauthorizedBusinessException("No puedes reseñar una reserva que no te pertenece.");

        bool isCompleted = booking.StatusId == 3;
        bool isConfirmedAndPassed = booking.StatusId == 2 && 
                                   DateTime.UtcNow > booking.SlotDate.ToDateTime(booking.SlotStartTime);

        if (!isCompleted && !isConfirmedAndPassed)
            throw new BusinessException("Solo puedes reseñar tours que ya hayan finalizado o estén marcados como completados.");

        var reviewNode = new ReviewNode
        {
            BookingId = booking.Id,
            UserId = userId,
            AttractionId = booking.AttractionId,
            Rating = request.OverallRating,
            Comment = request.Comment,
            Ratings = request.Ratings.Select(r => new ReviewRatingNode
            {
                Criteria = r.CriteriaId.ToString(),
                Rating = r.Score
            }).ToList()
        };

        var success = await _reviewData.AddReviewAsync(reviewNode);
        if (!success)
            throw new BusinessException("No se pudo guardar la reseña. Intente nuevamente.");

        return new ReviewResponse
        {
            Id = reviewNode.Id,
            ClientName = "User", // Enriquecer con nombre real si es necesario
            OverallRating = request.OverallRating,
            Title = request.Title,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow,
            Ratings = request.Ratings.Select(r => new CriteriaRatingResponse
            {
                CriteriaName = r.CriteriaId.ToString(),
                Score = r.Score
            }).ToList()
        };
    }

    public async Task<PagedResult<ReviewResponse>> GetByAttractionAsync(Guid attractionId, int page = 1, int pageSize = 10)
    {
        var filters = new QueryFilters { PageNumber = page, PageSize = pageSize };
        var paged = await _reviewData.GetReviewsByAttractionAsync(attractionId, filters);

        return new PagedResult<ReviewResponse>
        {
            Items = paged.Items.Select(MapToResponse).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    public async Task<PagedResult<ReviewResponse>> SearchManagementAsync(ReviewSearchRequest request, Guid currentUserId, bool isAdmin)
    {
        var filters = new ReviewQueryFilters
        {
            AttractionId = request.AttractionId,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var paged = await _reviewData.SearchReviewsAsync(filters);

        return new PagedResult<ReviewResponse>
        {
            Items = paged.Items.Select(MapToResponse).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    private static ReviewResponse MapToResponse(ReviewNode node) => new()
    {
        Id = node.Id,
        ClientName = node.ClientName,
        OverallRating = node.Rating,
        Comment = node.Comment,
        CreatedAt = node.CreatedAt,
        Ratings = node.Ratings.Select(r => new CriteriaRatingResponse
        {
            CriteriaName = r.Criteria,
            Score = r.Rating
        }).ToList()
    };

    public async Task<bool> DeleteReviewAsync(Guid reviewId, Guid userId, bool isAdmin)
    {
        if (!isAdmin)
            throw new UnauthorizedBusinessException("Solo los administradores pueden eliminar reseñas.");

        return await _reviewData.DeleteAsync(reviewId);
    }
}
