using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microservicios.Atracciones.Booking.DataAccess.Common;
using Microservicios.Atracciones.Booking.DataAccess.Entities;
using Microservicios.Atracciones.Booking.DataAccess.Repositories.Interfaces;
using Microservicios.Atracciones.Booking.DataManagement.Interfaces;
using Microservicios.Atracciones.Booking.DataManagement.Models;

namespace Microservicios.Atracciones.Booking.DataManagement.Services;

public class ReviewDataService : IReviewDataService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ReviewDataService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<bool> AddReviewAsync(ReviewNode reviewNode)
    {
        var entity = _mapper.Map<Review>(reviewNode);
        
        // 1. Guardar Review
        await _uow.Reviews.AddAsync(entity);
        var success = await _uow.CompleteAsync() > 0;
        
        // TODO: En el microservicio desacoplado, la actualización del rating promedio 
        // en la tabla de Atracciones debería ser manejada mediante eventos (ej. ReviewCreatedEvent)
        // ya que la tabla Attraction ya no reside en este DbContext.
        
        return success;
    }

    public async Task<decimal> GetAverageRatingByAttractionAsync(Guid attractionId)
    {
        var avg = await _uow.Reviews.Query()
            .Where(r => r.AttractionId == attractionId)
            .AverageAsync(r => (decimal?)r.OverallScore);
            
        return avg ?? 0;
    }

    public async Task<PagedResult<ReviewNode>> GetReviewsByAttractionAsync(Guid attractionId, QueryFilters filters)
    {
        IQueryable<Review> query = _uow.Reviews.Query()
            .Include(r => r.Booking)
            .Where(r => r.AttractionId == attractionId);

        var totalCount = await query.CountAsync();
        
        var items = await query.OrderByDescending(r => r.CreatedAt)
                               .Skip(filters.Offset)
                               .Take(filters.PageSize)
                               .ToListAsync();

        return new PagedResult<ReviewNode>
        {
            Items = _mapper.Map<IEnumerable<ReviewNode>>(items).ToList(),
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    public async Task<PagedResult<ReviewNode>> SearchReviewsAsync(ReviewQueryFilters filters)
    {
        IQueryable<Review> query = _uow.Reviews.Query()
            .Include(r => r.Booking);

        // TODO: Filtrado por ManagedById (Partner) requeriría lista de AttractionIds desde Catálogo.
        
        if (filters.AttractionId.HasValue)
        {
            query = query.Where(r => r.AttractionId == filters.AttractionId.Value);
        }

        var totalCount = await query.CountAsync();
        
        var items = await query.OrderByDescending(x => x.CreatedAt)
                               .Skip(filters.Offset)
                               .Take(filters.PageSize)
                               .ToListAsync();

        return new PagedResult<ReviewNode>
        {
            Items = _mapper.Map<IEnumerable<ReviewNode>>(items).ToList(),
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    public async Task<bool> DeleteAsync(Guid reviewId)
    {
        var entity = await _uow.Reviews.GetByIdAsync(reviewId);
        if (entity == null) return false;

        _uow.Reviews.Delete(entity);
        return await _uow.CompleteAsync() > 0;
    }
}
