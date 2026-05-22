using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microservicios.Atracciones.Booking.DataAccess.Context;
using Microservicios.Atracciones.Booking.DataAccess.Repositories;
using Microservicios.Atracciones.Booking.DataAccess.Repositories.Interfaces;

namespace Microservicios.Atracciones.Booking.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ── DbContext ──
        services.AddDbContext<AtraccionDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
        });

        // ── Unit of Work ──
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Repositorio genérico ──
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        return services;
    }
}
