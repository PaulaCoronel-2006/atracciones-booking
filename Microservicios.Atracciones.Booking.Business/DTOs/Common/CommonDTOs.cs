namespace Microservicios.Atracciones.Booking.Business.DTOs.Common;

public class TagResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class InclusionResponse
{
    public Guid Id { get; set; }
    public string DefaultText { get; set; } = string.Empty;
    public string? IconSlug { get; set; }
}

public class LanguageResponse
{
    public short Id { get; set; }
    public string IsoCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CreateTagRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public string Name { get; set; } = string.Empty;
}

public class CreateInclusionRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public string DefaultText { get; set; } = string.Empty;
    public string? IconSlug { get; set; }
    public short LanguageId { get; set; } = 1;
}

public class TicketCategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public short? AgeRangeMin { get; set; }
    public short? AgeRangeMax { get; set; }
}

public class CreateTicketCategoryRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public short? AgeRangeMin { get; set; }
    public short? AgeRangeMax { get; set; }
    public short SortOrder { get; set; } = 0;
}
