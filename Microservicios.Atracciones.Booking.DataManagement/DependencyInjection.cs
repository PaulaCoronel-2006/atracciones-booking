using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microservicios.Atracciones.Booking.DataManagement.Interfaces;
using Microservicios.Atracciones.Booking.DataManagement.Services;
using System.Reflection;

namespace Microservicios.Atracciones.Booking.DataManagement;

public static class DependencyInjection
{
    public static IServiceCollection AddDataManagementServices(this IServiceCollection services)
    {
        // 1. Configurar Mapster
        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        typeAdapterConfig.Scan(Assembly.GetExecutingAssembly());
        
        services.AddSingleton(typeAdapterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        // 2. Registrar Data Services
        services.AddScoped<IInventoryDataService, InventoryDataService>();
        services.AddScoped<IBookingDataService, BookingDataService>();
        services.AddScoped<IReviewDataService, ReviewDataService>();

        return services;
    }
}
