namespace Microservicios.Atracciones.Booking.Business.Exceptions;

/// <summary>
/// Excepción lanzada cuando los datos de entrada no cumplen con las reglas de validación.
/// Contiene la lista de errores descriptivos para devolver al cliente.
/// </summary>
[System.Diagnostics.DebuggerNonUserCode]
public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors)
        : base("Uno o más errores de validación ocurrieron.")
    {
        Errors = errors;
    }

    public ValidationException(string error)
        : base(error)
    {
        Errors = [error];
    }
}
