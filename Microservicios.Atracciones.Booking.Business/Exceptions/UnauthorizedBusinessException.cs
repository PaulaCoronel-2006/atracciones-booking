namespace Microservicios.Atracciones.Booking.Business.Exceptions;

[System.Diagnostics.DebuggerNonUserCode]
public class UnauthorizedBusinessException : Exception
{
    public UnauthorizedBusinessException(string message) : base(message) { }
}
