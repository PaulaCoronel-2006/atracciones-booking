namespace Microservicios.Atracciones.Booking.Business.DTOs.Review;

// ─── REQUEST ─────────────────────────────────────────────────────────────────

/// <summary>Petición de creación de reseña por parte del cliente.</summary>
public class CreateReviewRequest
{
    public string PnrCode { get; set; } = string.Empty;
    public byte OverallRating { get; set; }     // 1 - 5
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public short? LanguageId { get; set; }
    public List<CriteriaRatingRequest> Ratings { get; set; } = [];
}

public class CriteriaRatingRequest
{
    public short CriteriaId { get; set; }
    public byte Score { get; set; }             // 1 - 5
}

/// <summary>Filtros para búsqueda administrativa de reseñas.</summary>
public class ReviewSearchRequest
{
    public Guid? AttractionId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// ─── RESPONSE ────────────────────────────────────────────────────────────────

public class ReviewResponse
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public byte OverallRating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public string? OperatorResponse { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CriteriaRatingResponse> Ratings { get; set; } = [];
}

public class CriteriaRatingResponse
{
    public string CriteriaName { get; set; } = string.Empty;
    public byte Score { get; set; }
}
