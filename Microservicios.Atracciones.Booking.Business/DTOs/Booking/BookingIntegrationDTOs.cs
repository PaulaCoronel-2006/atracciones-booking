namespace Microservicios.Atracciones.Booking.Business.DTOs.Booking;

// ══════════════════════════════════════════════════════════════
// WRAPPER ESTÁNDAR DE RESPUESTA (Compatible con todos los servicios)
// ══════════════════════════════════════════════════════════════

/// <summary>
/// Envoltorio genérico de respuesta REST. Todos los endpoints del contrato
/// de integración con Booking deben retornar este tipo.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = [];

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? [] };
}

/// <summary>
/// Respuesta paginada con metadatos de navegación.
/// </summary>
public class PagedApiResponse<T>
{
    public bool Success { get; set; }
    public List<T> Data { get; set; } = [];
    public PaginationMeta Meta { get; set; } = new();
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = [];

    public static PagedApiResponse<T> Ok(List<T> data, int totalItems, int page, int pageSize) =>
        new()
        {
            Success = true,
            Data = data,
            Meta = new PaginationMeta
            {
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize
            }
        };
}

public class PaginationMeta
{
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}

// ══════════════════════════════════════════════════════════════
// DTO: ATRACCIÓN PARA INTEGRACIÓN CON BOOKING
// ══════════════════════════════════════════════════════════════

/// <summary>
/// DTO de atracción para el contrato de integración con el sistema de Booking.
/// Sigue el estándar { id, nombre, descripcion, precio, moneda, ubicacion, imagenUrl, disponible }
/// </summary>
public class AtraccionBookingDto
{
    // ── Campos básicos requeridos por Booking.productos ──────────

    /// <summary>
    /// ID de la atracción. Se almacena en productos.id_externo en el sistema de Booking.
    /// </summary>
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    /// <summary>
    /// Precio mínimo disponible.
    /// Se almacena en productos.precio en Booking para filtros y ordenamiento.
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>Código de moneda ISO 4217 (Ej: "USD").</summary>
    public string Moneda { get; set; } = "USD";

    public string Ubicacion { get; set; } = string.Empty;

    /// <summary>
    /// URL de la imagen principal (is_main = true en attraction_media).
    /// Se almacena en productos.imagen_url en Booking.
    /// </summary>
    public string? ImagenUrl { get; set; }

    /// <summary>
    /// True si la atracción está publicada y tiene al menos un slot futuro disponible.
    /// Se almacena en productos.disponible en Booking.
    /// </summary>
    public bool Disponible { get; set; }
}

// ══════════════════════════════════════════════════════════════
// DTO: DISPONIBILIDAD (Agrupada por día — compatible con Booking)
// ══════════════════════════════════════════════════════════════

/// <summary>
/// Disponibilidad diaria de una atracción.
/// Agrupa todos los slots de horarios de un día en un solo cupo total.
/// Compatible con la tabla disponibilidad_productos del sistema de Booking.
/// </summary>
public class DisponibilidadDiariaDto
{
    /// <summary>Fecha en formato yyyy-MM-dd</summary>
    public string Fecha { get; set; } = string.Empty;

    /// <summary>
    /// Suma de capacity_available de todos los slots activos de ese día.
    /// Se almacena en disponibilidad_productos.cupos_disponibles en Booking.
    /// </summary>
    public int CuposDisponibles { get; set; }

    /// <summary>Horarios disponibles en ese día con su detalle individual</summary>
    public List<HorarioDto> Horarios { get; set; } = [];
}

/// <summary>Slot individual de un horario (para uso en el frontend de atracciones)</summary>
public class HorarioDto
{
    public Guid SlotId { get; set; }
    public string HoraInicio { get; set; } = string.Empty; // Formato HH:mm
    public string? HoraFin { get; set; }
    public int CuposDisponibles { get; set; }
    public int CuposTotales { get; set; }
}

// ══════════════════════════════════════════════════════════════
// REQUEST: FILTROS DE BÚSQUEDA PARA INTEGRACIÓN
// ══════════════════════════════════════════════════════════════

/// <summary>
/// Parámetros de búsqueda del contrato de integración con Booking.
/// </summary>
public class AtraccionSearchBookingRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? Ubicacion { get; set; }
    public bool? Disponible { get; set; }

    /// <summary>Filtrar por fecha de disponibilidad (yyyy-MM-dd)</summary>
    public DateOnly? Fecha { get; set; }

    public decimal? PrecioMinimo { get; set; }
    public decimal? PrecioMaximo { get; set; }
}

// ══════════════════════════════════════════════════════════════
// DTOs PARA TRANSACCIONES (RESERVAS)
// ══════════════════════════════════════════════════════════════

/// <summary>
/// Solicitud para crear una nueva reserva desde el Marketplace.
/// </summary>
public class AtraccionBookingRequestDto
{
    /// <summary>ID del slot de disponibilidad elegido.</summary>
    public Guid SlotId { get; set; }

    /// <summary>Lista de pasajeros en formato frontend (passengers[]).</summary>
    public List<PassengerBookingDto>? Passengers { get; set; }

    /// <summary>Lista de tickets en formato alternativo (tickets[]). Se usa si Passengers es null.</summary>
    public List<TicketBookingDetailDto>? Tickets { get; set; }

    // Snapshots del catálogo para persistencia autónoma
    public Guid AttractionId { get; set; }
    public Guid ProductOptionId { get; set; }
    public string? AttractionName { get; set; }
    public string? ProductTitle { get; set; }
    public string? Currency { get; set; }

    /// <summary>Nombre de contacto (va a billing.CustomerName si no se envía Billing).</summary>
    public string? ContactName { get; set; }

    /// <summary>Email de contacto (va a billing.Email si no se envía Billing).</summary>
    public string? ContactEmail { get; set; }

    /// <summary>Indica si es una venta desde POS (punto de venta físico).</summary>
    public bool IsPosSale { get; set; } = false;

    public string? Notas { get; set; }

    /// <summary>Información opcional para facturación. Si es null, se construye desde ContactName/Email.</summary>
    public BillingInfo? Billing { get; set; }

    /// <summary>Normaliza el DTO: convierte Passengers a Tickets y rellena Billing si faltan datos.</summary>
    public void Normalize()
    {
        // Si llegan passengers pero no tickets, convertir
        if ((Tickets == null || Tickets.Count == 0) && Passengers != null)
        {
            Tickets = [];
            foreach (var p in Passengers)
            {
                int count = p.Quantity > 0 ? p.Quantity : 1;
                for (int i = 0; i < count; i++)
                {
                    Tickets.Add(new TicketBookingDetailDto
                    {
                        TicketCategoryId = p.TicketCategoryId ?? Guid.Empty,
                        PriceTierId = p.PriceTierId,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        DocumentNumber = p.DocumentNumber ?? string.Empty,
                        DocumentType = p.DocumentType
                    });
                }
            }
        }

        // Si no hay billing pero hay contactName, construir billing básico
        if (Billing == null && !string.IsNullOrEmpty(ContactName))
        {
            Billing = new BillingInfo
            {
                CustomerName = ContactName,
                Email = ContactEmail
            };
        }
    }
}

/// <summary>
/// Formato de pasajero enviado por el frontend.
/// </summary>
public class PassengerBookingDto
{
    public Guid? TicketCategoryId { get; set; }
    public Guid? PriceTierId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string? DocumentType { get; set; }
    public int Quantity { get; set; } = 1;
}

public class BillingInfo
{
    public string? CustomerName { get; set; }
    public string? TaxId { get; set; } // RUC/Cédula
    public string? Email { get; set; }
    public string? Address { get; set; }
}

public class TicketBookingDetailDto
{
    /// <summary>ID de la categoría de ticket (Adulto, Niño, etc.)</summary>
    public Guid TicketCategoryId { get; set; }

    /// <summary>ID del PriceTier específico para asegurar el precio congelado (opcional)</summary>
    public Guid? PriceTierId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    
    // Snapshots para el detalle
    public decimal UnitPrice { get; set; }
    public string? PriceTierLabel { get; set; }
}

/// <summary>
/// Respuesta tras procesar una reserva.
/// </summary>
public class AtraccionBookingResponseDto
{
    public Guid BookingId { get; set; }
    
    /// <summary>Código localizador (PNR).</summary>
    public string PnrCode { get; set; } = string.Empty;
    
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    
    /// <summary>Fecha y hora de la actividad.</summary>
    public DateTime ActivityDate { get; set; }
    
    public string AttractionName { get; set; } = string.Empty;
    public string? AttractionImage { get; set; }
    public int TotalPassengers { get; set; }
}
