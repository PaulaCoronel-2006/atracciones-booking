namespace Microservicios.Atracciones.Booking.DataAccess.Common;

/// <summary>
/// Entidad base para todas las tablas principales del dominio.
/// Provee Id, auditoría básica y soporte para soft-delete.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
