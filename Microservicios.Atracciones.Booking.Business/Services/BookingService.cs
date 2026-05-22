using FluentValidation;
using Microservicios.Atracciones.Booking.Business.DTOs.Booking;
using Microservicios.Atracciones.Booking.Business.Exceptions;
using Microservicios.Atracciones.Booking.Business.Interfaces;
using Microservicios.Atracciones.Booking.DataAccess.Common;
using Microservicios.Atracciones.Booking.DataManagement.Interfaces;
using Microservicios.Atracciones.Booking.DataManagement.Models;

namespace Microservicios.Atracciones.Booking.Business.Services;

public class BookingService : IBookingService
{
    private readonly IBookingDataService _bookingData;
    private readonly IInventoryDataService _inventoryData;
    private readonly IValidator<CreateBookingRequest> _createValidator;
    private readonly IValidator<CancelBookingRequest> _cancelValidator;

    public BookingService(
        IBookingDataService bookingData,
        IInventoryDataService inventoryData,
        IValidator<CreateBookingRequest> createValidator,
        IValidator<CancelBookingRequest> cancelValidator)
    {
        _bookingData = bookingData;
        _inventoryData = inventoryData;
        _createValidator = createValidator;
        _cancelValidator = cancelValidator;
    }

    public async Task<BookingConfirmationResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new Exceptions.ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var slot = await _inventoryData.GetSlotByIdAsync(request.SlotId);
        if (slot == null)
            throw new NotFoundException("Slot de disponibilidad", request.SlotId);

        if (slot.SlotDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessException("No se puede reservar en un slot de fecha pasada.");

        short totalPassengers = (short)request.Passengers.Sum(p => p.Quantity);
        if (slot.CapacityAvailable < totalPassengers)
            throw new BusinessException($"No hay suficiente disponibilidad. Disponible: {slot.CapacityAvailable}, Solicitado: {totalPassengers}.");

        // En el microservicio desacoplado, la validación de PriceTiers y el cálculo de precios
        // debería hacerse consultando al servicio de Catálogo (vía gRPC o caché) o 
        // confiando en los datos del frontend si hay una validación posterior.
        // Por ahora, asumimos que los precios vienen en el request o los simplificamos.
        
        // TODO: Implementar llamada a Catálogo Microservice para validar precios y nombres.
        
        decimal totalAmount = request.Passengers.Sum(p => p.UnitPrice * p.Quantity);
        string currencyCode = "USD"; // Debería venir del Catálogo

        var bookingNode = new BookingNode
        {
            UserId = userId,
            SlotId = request.SlotId,
            StatusId = 1,
            TotalAmount = totalAmount,
            CurrencyCode = currencyCode,
            Notes = request.Notes,
            AttractionId = request.AttractionId,
            Details = request.Passengers.Select(p => new BookingDetailNode
            {
                PriceTierId = p.PriceTierId,
                PriceTierLabel = p.PriceTierLabel,
                AttractionName = request.AttractionName,
                ProductTitle = request.ProductTitle,
                FirstName = p.FirstName,
                LastName = p.LastName,
                DocumentType = p.DocumentType,
                DocumentNumber = p.DocumentNumber,
                Quantity = p.Quantity,
                UnitPrice = p.UnitPrice
            }).ToList()
        };

        var created = await _bookingData.CreateBookingAsync(bookingNode);
        if (created == null)
            throw new BusinessException("No se pudo completar la reserva. Intente nuevamente.");

        await _inventoryData.DecrementSlotCapacityAsync(request.SlotId, totalPassengers);

        return new BookingConfirmationResponse
        {
            Id = created.Id,
            PnrCode = created.PnrCode,
            StatusName = created.StatusName,
            TotalAmount = created.TotalAmount,
            CurrencyCode = created.CurrencyCode,
            AttractionName = created.AttractionName,
            SlotDate = created.SlotDate,
            SlotStartTime = created.SlotStartTime,
            TotalPassengers = totalPassengers,
            CreatedAt = created.CreatedAt
        };
    }

    public async Task<BookingDetailResponse> GetByPnrAsync(string pnrCode)
    {
        var booking = await _bookingData.GetByPnrAsync(pnrCode.ToUpperInvariant());
        if (booking == null)
            throw new NotFoundException("Reserva", pnrCode);

        return MapToDetail(booking);
    }

    public async Task<BookingDetailResponse> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin)
    {
        var booking = await _bookingData.GetByIdAsync(id);
        if (booking == null)
            throw new NotFoundException("Reserva", id);

        if (!isAdmin && booking.UserId != currentUserId)
            throw new UnauthorizedBusinessException("No tienes permiso para ver esta reserva.");

        return MapToDetail(booking);
    }

    public async Task<PagedResult<BookingSummaryResponse>> GetUserHistoryAsync(Guid userId, int page = 1, int pageSize = 10)
    {
        var filters = new QueryFilters { PageNumber = page, PageSize = pageSize };
        var paged = await _bookingData.GetBookingsByUserAsync(userId, filters);

        return new PagedResult<BookingSummaryResponse>
        {
            Items = paged.Items.Select(MapToSummary).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    public async Task<PagedResult<BookingSummaryResponse>> SearchManagementAsync(BookingSearchRequest request, Guid userId, bool isAdmin)
    {
        var filter = new BookingQueryFilters
        {
            SearchTerm = request.SearchTerm,
            StatusId = request.StatusId,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            ManagedById = isAdmin ? null : userId
        };

        var paged = await _bookingData.SearchBookingsAsync(filter);

        return new PagedResult<BookingSummaryResponse>
        {
            Items = paged.Items.Select(MapToSummary).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }

    public async Task CancelBookingAsync(Guid userId, bool isAdmin, CancelBookingRequest request)
    {
        var validation = await _cancelValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new Exceptions.ValidationException(validation.Errors.Select(e => e.ErrorMessage).ToList());

        var booking = await _bookingData.GetByPnrAsync(request.PnrCode.ToUpperInvariant());
        if (booking == null)
            throw new NotFoundException("Reserva", request.PnrCode);

        if (!isAdmin && booking.UserId != userId)
            throw new UnauthorizedBusinessException("No tienes permiso para cancelar esta reserva.");

        if (booking.StatusId == 4)
            throw new BusinessException("La reserva ya está cancelada.");
        
        if (booking.StatusId == 3)
            throw new BusinessException("No se puede cancelar una reserva ya completada.");

        var ok = await _bookingData.UpdateBookingStatusAsync(booking.Id, statusId: 4, request.CancelReason);
        if (!ok)
            throw new BusinessException("No se pudo cancelar la reserva. Intente nuevamente.");

        short totalPassengers = (short)booking.Details.Sum(d => d.Quantity);
        await _inventoryData.DecrementSlotCapacityAsync(booking.SlotId, (short)-totalPassengers);
    }

    private static BookingDetailResponse MapToDetail(BookingNode b) => new()
    {
        Id = b.Id,
        PnrCode = b.PnrCode,
        StatusName = b.StatusName,
        TotalAmount = b.TotalAmount,
        CurrencyCode = b.CurrencyCode,
        Notes = b.Notes,
        CreatedAt = b.CreatedAt,
        AttractionName = b.AttractionName,
        SlotDate = b.SlotDate,
        SlotStartTime = b.SlotStartTime,
        Passengers = b.Details.Select(d => new PassengerDetailResponse
        {
            FullName = $"{d.FirstName} {d.LastName}",
            DocumentNumber = d.DocumentNumber,
            PriceTierLabel = d.PriceTierLabel,
            UnitPrice = d.UnitPrice,
            Quantity = d.Quantity
        }).ToList(),
        CanReview = CalculateCanReview(b)
    };

    private static BookingSummaryResponse MapToSummary(BookingNode b) => new()
    {
        Id = b.Id,
        PnrCode = b.PnrCode,
        AttractionName = b.AttractionName,
        StatusName = b.StatusName,
        StatusId = b.StatusId,
        TotalAmount = b.TotalAmount,
        CurrencyCode = b.CurrencyCode,
        SlotDate = b.SlotDate,
        SlotStartTime = b.SlotStartTime,
        TotalPassengers = b.Details.Sum(d => d.Quantity),
        CreatedAt = b.CreatedAt,
        CanReview = CalculateCanReview(b),
        Tickets = b.Details.Select(d => new BookingTicketSummary
        {
            CategoryName = d.PriceTierLabel,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice
        }).ToList()
    };

    private static bool CalculateCanReview(BookingNode b)
    {
        if (b.StatusId == 3) return true;
        if (b.StatusId == 2)
        {
            var slotDateTime = b.SlotDate.ToDateTime(b.SlotStartTime);
            return DateTime.UtcNow > slotDateTime;
        }
        return false;
    }
}
