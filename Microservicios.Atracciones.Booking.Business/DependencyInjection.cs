using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microservicios.Atracciones.Booking.Business.DTOs.Booking;
using Microservicios.Atracciones.Booking.Business.DTOs.Review;
using Microservicios.Atracciones.Booking.Business.Interfaces;
using Microservicios.Atracciones.Booking.Business.Services;
using Microservicios.Atracciones.Booking.Business.Validators;

namespace Microservicios.Atracciones.Booking.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // ─── Servicios de negocio ──────────────────────────────────────────────
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IStorageService, LocalStorageService>();
        services.AddScoped<IBookingIntegrationService, BookingIntegrationService>();

        // ─── Validadores FluentValidation ──────────────────────────────────────
        services.AddScoped<IValidator<CreateBookingRequest>, CreateBookingValidator>();
        services.AddScoped<IValidator<CancelBookingRequest>, CancelBookingValidator>();
        services.AddScoped<IValidator<CreateReviewRequest>, CreateReviewValidator>();

        return services;
    }
}
