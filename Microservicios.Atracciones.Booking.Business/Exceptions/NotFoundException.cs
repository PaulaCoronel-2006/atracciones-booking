
namespace Microservicios.Atracciones.Booking.Business.Exceptions
{
    [System.Diagnostics.DebuggerNonUserCode]
    public class NotFoundException : BusinessException
    {
        public NotFoundException(string entity, object key)
            : base($"No se encontró {entity} con la llave: {key}") { }
    }
}
