using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microservicios.Atracciones.Booking.Business.DTOs.Booking;
using Microservicios.Atracciones.Booking.Business.Interfaces;
using Microservicios.Atracciones.Booking.DataAccess.Repositories.Interfaces;
using Microservicios.Atracciones.Booking.DataManagement.Interfaces;

namespace Microservicios.Atracciones.Booking.Business.Services;

/// <summary>
/// Implementación del servicio de integración con el sistema central de Booking.
/// Convierte los datos internos de la plataforma de atracciones al formato estándar del contrato.
/// </summary>
public class BookingIntegrationService : IBookingIntegrationService
{
    private readonly IInventoryDataService _inventoryData;
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _configuration;

    public BookingIntegrationService(
        IInventoryDataService inventoryData,
        IUnitOfWork uow,
        IConfiguration configuration)
    {
        _inventoryData = inventoryData;
        _uow = uow;
        _configuration = configuration;
    }

    // ══════════════════════════════════════════════════
    // DISPONIBILIDAD AGRUPADA POR DÍA
    // ══════════════════════════════════════════════════

    public async Task<ApiResponse<List<DisponibilidadDiariaDto>>> ObtenerDisponibilidadAsync(Guid attractionId, DateOnly? fecha = null)
    {
        var fechaInicio = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var fechaFin = fecha.HasValue
            ? fecha.Value
            : DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var slots = await _uow.AvailabilitySlots.Query()
            .Where(s =>
                s.ProductId == attractionId &&
                s.IsActive &&
                s.SlotDate >= fechaInicio &&
                s.SlotDate <= fechaFin &&
                s.CapacityAvailable > 0)
            .OrderBy(s => s.SlotDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        var disponibilidadPorDia = slots
            .GroupBy(s => s.SlotDate)
            .Select(g => new DisponibilidadDiariaDto
            {
                Fecha = g.Key.ToString("yyyy-MM-dd"),
                CuposDisponibles = g.Sum(s => s.CapacityAvailable),
                Horarios = g.Select(s => new HorarioDto
                {
                    SlotId = s.Id,
                    HoraInicio = s.StartTime.ToString(@"HH\:mm"),
                    HoraFin = s.EndTime?.ToString(@"HH\:mm"),
                    CuposDisponibles = s.CapacityAvailable,
                    CuposTotales = s.CapacityTotal
                }).ToList()
            })
            .ToList();

        return ApiResponse<List<DisponibilidadDiariaDto>>.Ok(disponibilidadPorDia);
    }

    // ══════════════════════════════════════════════════
    // TRANSACCIONES: CREAR RESERVA
    // ══════════════════════════════════════════════════

    public async Task<ApiResponse<AtraccionBookingResponseDto>> CrearReservaAsync(AtraccionBookingRequestDto request, Guid userId)
    {
        var slot = await _uow.AvailabilitySlots.Query()
            .FirstOrDefaultAsync(s => s.Id == request.SlotId && s.IsActive);

        if (slot == null)
            return ApiResponse<AtraccionBookingResponseDto>.Fail("El horario seleccionado ya no está disponible.");

        int totalTickets = request.Tickets!.Count;
        if (slot.CapacityAvailable < totalTickets)
            return ApiResponse<AtraccionBookingResponseDto>.Fail($"No hay cupos suficientes. Cupos restantes: {slot.CapacityAvailable}");

        decimal totalAmount = 0;
        var details = new List<DataAccess.Entities.BookingDetail>();
        string currency = request.Currency ?? "USD";

        foreach (var t in request.Tickets)
        {
            // En el microservicio desacoplado, confiamos en los datos de precio y categoría 
            // que vienen del request (snapshot del catálogo)
            totalAmount += t.UnitPrice;

            details.Add(new DataAccess.Entities.BookingDetail
            {
                Id = Guid.NewGuid(),
                ProductOptionId = request.ProductOptionId,
                PriceTierId = t.PriceTierId ?? Guid.Empty,
                FirstName = t.FirstName,
                LastName = t.LastName,
                DocumentNumber = t.DocumentNumber,
                DocumentType = t.DocumentType,
                Quantity = 1,
                UnitPrice = t.UnitPrice,
                CurrencyCode = currency,
                AttractionNameSnapshot = request.AttractionName ?? "Attraction",
                OptionNameSnapshot = request.ProductTitle ?? "Option",
                TierNameSnapshot = t.PriceTierLabel ?? "Ticket"
            });
        }

        decimal taxRate = 0.15m;
        try {
            var configValue = _configuration["Billing:TaxRate"];
            if (!string.IsNullOrEmpty(configValue)) {
                taxRate = decimal.Parse(configValue, System.Globalization.CultureInfo.InvariantCulture);
            }
        } catch { }
        
        totalAmount = totalAmount * (1 + taxRate);

        var booking = new DataAccess.Entities.Booking
        {
            Id = Guid.NewGuid(),
            PnrCode = GeneratePnrCode(),
            UserId = userId,
            AttractionId = request.AttractionId,
            SlotId = slot.Id,
            StatusId = 2, // Confirmed
            TotalAmount = Math.Round(totalAmount, 2),
            CurrencyCode = currency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try 
        {
            await _uow.Bookings.AddAsync(booking);
            
            foreach (var detail in details)
            {
                detail.BookingId = booking.Id;
                await _uow.BookingDetails.AddAsync(detail);
            }

            slot.CapacityAvailable -= (short)totalTickets;
            slot.UpdatedAt = DateTime.UtcNow;

            await _uow.CompleteAsync();

            return ApiResponse<AtraccionBookingResponseDto>.Ok(new AtraccionBookingResponseDto
            {
                BookingId = booking.Id,
                PnrCode = booking.PnrCode,
                Status = "Confirmed",
                TotalAmount = totalAmount,
                Currency = booking.CurrencyCode,
                ActivityDate = slot.SlotDate.ToDateTime(slot.StartTime),
                AttractionName = request.AttractionName ?? "Attraction"
            }, "Reserva creada exitosamente.");
        }
        catch (Exception ex)
        {
            var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return ApiResponse<AtraccionBookingResponseDto>.Fail("Error interno al procesar la reserva: " + errorMsg);
        }
    }

    public async Task<ApiResponse<bool>> CancelarReservaAsync(Guid bookingId, Guid userId)
    {
        var booking = await _uow.Bookings.Query()
            .Include(b => b.AvailabilitySlot)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

        if (booking == null)
            return ApiResponse<bool>.Fail("Reserva no encontrada.");

        if (booking.StatusId == 4)
            return ApiResponse<bool>.Fail("La reserva ya se encuentra cancelada.");

        if (booking.StatusId == 3)
            return ApiResponse<bool>.Fail("No se puede cancelar una reserva que ya ha sido completada.");

        booking.StatusId = 4;
        booking.CancelledAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        var totalTickets = await _uow.BookingDetails.Query()
            .Where(d => d.BookingId == bookingId)
            .SumAsync(d => d.Quantity);
        booking.AvailabilitySlot.CapacityAvailable += (short)totalTickets;
        booking.AvailabilitySlot.UpdatedAt = DateTime.UtcNow;

        await _uow.CompleteAsync();

        return ApiResponse<bool>.Ok(true, "Reserva cancelada y cupos liberados.");
    }

    public async Task<ApiResponse<List<AtraccionBookingResponseDto>>> ListarMisReservasAsync(Guid userId)
    {
        var bookings = await _uow.Bookings.Query()
            .Include(b => b.AvailabilitySlot)
            .Include(b => b.Details)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var dtos = bookings.Select(b => new AtraccionBookingResponseDto
        {
            BookingId = b.Id,
            PnrCode = b.PnrCode,
            Status = b.StatusId switch { 1 => "Pending", 2 => "Confirmed", 3 => "Completed", 4 => "Cancelled", _ => "Unknown" },
            TotalAmount = b.TotalAmount,
            Currency = b.CurrencyCode,
            ActivityDate = b.AvailabilitySlot.SlotDate.ToDateTime(b.AvailabilitySlot.StartTime),
            AttractionName = b.Details.FirstOrDefault()?.AttractionNameSnapshot ?? "Attraction"
        }).ToList();

        return ApiResponse<List<AtraccionBookingResponseDto>>.Ok(dtos);
    }

    private string GeneratePnrCode()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }
}
