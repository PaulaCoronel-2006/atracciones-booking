using FluentValidation;
using Microservicios.Atracciones.Booking.Business.DTOs.Booking;

namespace Microservicios.Atracciones.Booking.Business.Validators;

public class CreateBookingValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.SlotId)
            .NotEmpty().WithMessage("El slot de reserva es requerido.");

        RuleFor(x => x.Passengers)
            .NotEmpty().WithMessage("Debe incluir al menos un pasajero.")
            .Must(p => p.Count <= 30).WithMessage("No se pueden reservar más de 30 pasajeros a la vez.");

        RuleForEach(x => x.Passengers).SetValidator(new PassengerValidator());
    }
}

public class PassengerValidator : AbstractValidator<BookingPassengerRequest>
{
    public PassengerValidator()
    {
        RuleFor(x => x.PriceTierId)
            .NotEmpty().WithMessage("Cada pasajero debe tener un precio/categoría asignado.");

        RuleFor(x => x.FirstName)
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .MaximumLength(100);

        RuleFor(x => x.DocumentNumber)
            .MaximumLength(50);

        RuleFor(x => x.Quantity)
            .GreaterThan((short)0).WithMessage("La cantidad debe ser mayor a 0.");
    }
}

public class CancelBookingValidator : AbstractValidator<CancelBookingRequest>
{
    public CancelBookingValidator()
    {
        RuleFor(x => x.PnrCode)
            .NotEmpty().WithMessage("El código PNR es requerido.")
            .Length(8).WithMessage("El código PNR debe tener 8 caracteres.");

        RuleFor(x => x.CancelReason)
            .MaximumLength(500).When(x => x.CancelReason != null)
            .WithMessage("El motivo de cancelación no puede exceder 500 caracteres.");
    }
}
