using FluentValidation;
using Microservicios.Atracciones.Booking.Business.DTOs.Review;

namespace Microservicios.Atracciones.Booking.Business.Validators;

public class CreateReviewValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.PnrCode)
            .NotEmpty().WithMessage("El código PNR de la reserva es requerido.")
            .Length(8).WithMessage("El código PNR debe tener 8 caracteres.");

        RuleFor(x => x.OverallRating)
            .InclusiveBetween((byte)1, (byte)5)
            .WithMessage("La calificación general debe estar entre 1 y 5.");

        RuleFor(x => x.Title)
            .MaximumLength(150).When(x => x.Title != null)
            .WithMessage("El título no puede exceder 150 caracteres.");

        RuleFor(x => x.Comment)
            .MaximumLength(3000).When(x => x.Comment != null)
            .WithMessage("El comentario no puede exceder 3000 caracteres.");

        RuleForEach(x => x.Ratings).SetValidator(new CriteriaRatingValidator());
    }
}

public class CriteriaRatingValidator : AbstractValidator<CriteriaRatingRequest>
{
    public CriteriaRatingValidator()
    {
        RuleFor(x => x.CriteriaId)
            .GreaterThan((short)0).WithMessage("ID de criterio inválido.");

        RuleFor(x => x.Score)
            .InclusiveBetween((byte)1, (byte)5)
            .WithMessage("La puntuación por criterio debe estar entre 1 y 5.");
    }
}
