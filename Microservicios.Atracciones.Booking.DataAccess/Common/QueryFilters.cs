namespace Microservicios.Atracciones.Booking.DataAccess.Common;

/// <summary>
/// Filtros genéricos para consultas paginadas.
/// Se especializa por entidad en la capa de Queries.
/// </summary>
public class QueryFilters
{
    private int _pageSize = 10;
    private int _pageNumber = 1;

    public string? SearchTerm { get; set; }

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value is < 1 or > 100 ? 10 : value;
    }

    public string? SortBy { get; set; }
    public bool IsAscending { get; set; } = true;

    /// <summary>Offset calculado para Skip/Take.</summary>
    public int Offset => (PageNumber - 1) * PageSize;
}

/// <summary>
/// Filtros específicos para búsqueda de atracciones.
/// </summary>
public class AttractionQueryFilters : QueryFilters
{
    public Guid? LocationId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategorySlug { get; set; }
    public Guid? SubcategoryId { get; set; }
    public Guid? TagId { get; set; }
    public string? TagIds { get; set; } // Comma separated
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinRating { get; set; }
    public short? LanguageId { get; set; }
    public string? LanguageIds { get; set; } // Comma separated
    public string? DifficultyLevel { get; set; }
    public string? DifficultyLevels { get; set; } // Comma separated
    public bool? IsPublished { get; set; } = true;
    public bool? IsActive { get; set; } = true;
    public Guid? ManagedById { get; set; }
}

/// <summary>
/// Filtros específicos para búsqueda de disponibilidad.
/// </summary>
public class AvailabilityQueryFilters : QueryFilters
{
    public Guid ProductId { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public int MinCapacity { get; set; } = 1;
}

/// <summary>Filtros específicos para gestión de reservas.</summary>
public class BookingQueryFilters : QueryFilters
{
    public Guid? ManagedById { get; set; }
    public short? StatusId { get; set; }
}

/// <summary>Filtros específicos para gestión de reseñas.</summary>
public class ReviewQueryFilters : QueryFilters
{
    public Guid? ManagedById { get; set; }
    public Guid? AttractionId { get; set; }
}
