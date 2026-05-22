using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microservicios.Atracciones.Booking.DataAccess.Common;
using Microservicios.Atracciones.Booking.DataAccess.Entities;
using Microservicios.Atracciones.Booking.DataAccess.Repositories.Interfaces;
using Microservicios.Atracciones.Booking.DataManagement.Interfaces;
using Microservicios.Atracciones.Booking.DataManagement.Models;
using BookingEntity = Microservicios.Atracciones.Booking.DataAccess.Entities.Booking;

namespace Microservicios.Atracciones.Booking.DataManagement.Services;

public class BookingDataService : IBookingDataService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public BookingDataService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<BookingNode?> CreateBookingAsync(BookingNode bookingNode)
    {
        var entity = _mapper.Map<BookingEntity>(bookingNode);
        
        string pnr;
        bool exists;
        do
        {
            pnr = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            exists = await _uow.Bookings.Query().AnyAsync(b => b.PnrCode == pnr);
        } 
        while (exists);

        entity.PnrCode = pnr;
        
        if(entity.StatusId == 0)
            entity.StatusId = 1;

        await _uow.Bookings.AddAsync(entity);
        
        var success = await _uow.CompleteAsync() > 0;
        
        return success ? await GetByPnrAsync(entity.PnrCode) : null;
    }

    public async Task<BookingNode?> GetByPnrAsync(string pnrCode)
    {
        var entity = await _uow.Bookings.Query()
            .Include(b => b.Status)
            .Include(b => b.AvailabilitySlot)
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.PnrCode == pnrCode);

        return entity == null ? null : _mapper.Map<BookingNode>(entity);
    }

    public async Task<PagedResult<BookingNode>> GetBookingsByUserAsync(Guid userId, QueryFilters filters)
    {
        IQueryable<BookingEntity> query = _uow.Bookings.Query()
            .Include(b => b.Status)
            .Include(b => b.AvailabilitySlot)
            .Include(b => b.Details)
            .Where(b => b.UserId == userId);

        var totalCount = await query.CountAsync();
        
        var items = await query.OrderByDescending(x => x.CreatedAt)
                               .Skip(filters.Offset)
                               .Take(filters.PageSize)
                               .ToListAsync();

        return new PagedResult<BookingNode>
        {
            Items = _mapper.Map<IEnumerable<BookingNode>>(items).ToList(),
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    public async Task<PagedResult<BookingNode>> SearchBookingsAsync(BookingQueryFilters filters)
    {
        IQueryable<BookingEntity> query = _uow.Bookings.Query()
            .Include(b => b.Status)
            .Include(b => b.AvailabilitySlot);

        // TODO: En el microservicio desacoplado, el filtrado por ManagedById (Partner)
        // requeriría una lista de AttractionIds que el partner administra, obtenida del servicio de Catálogo.
        
        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            query = query.Where(b => b.PnrCode.Contains(filters.SearchTerm));
        }

        if (filters.StatusId.HasValue)
        {
            query = query.Where(b => b.StatusId == filters.StatusId.Value);
        }

        var totalCount = await query.CountAsync();
        
        var items = await query.OrderByDescending(x => x.CreatedAt)
                               .Skip(filters.Offset)
                               .Take(filters.PageSize)
                               .ToListAsync();

        return new PagedResult<BookingNode>
        {
            Items = _mapper.Map<IEnumerable<BookingNode>>(items).ToList(),
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    public async Task<bool> UpdateBookingStatusAsync(Guid bookingId, short statusId, string? cancelReason = null)
    {
        var entity = await _uow.Bookings.GetByIdAsync(bookingId);
        if (entity == null) return false;

        entity.StatusId = statusId;
        
        if (statusId == 4) 
        {
            entity.CancelledAt = DateTime.UtcNow;
            entity.CancelReason = cancelReason;
        }

        _uow.Bookings.Update(entity);
        return await _uow.CompleteAsync() > 0;
    }

    public async Task<BookingNode?> GetByIdAsync(Guid id)
    {
        var entity = await _uow.Bookings.Query()
            .Include(b => b.Status)
            .Include(b => b.AvailabilitySlot)
            .Include(b => b.Details)
            .FirstOrDefaultAsync(b => b.Id == id);

        return entity == null ? null : _mapper.Map<BookingNode>(entity);
    }
}
