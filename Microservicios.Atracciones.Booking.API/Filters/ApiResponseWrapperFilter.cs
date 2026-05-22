using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microservicios.Atracciones.Booking.Business.DTOs.Booking;

namespace Microservicios.Atracciones.Booking.API.Filters;

/// <summary>
/// Intercepta las respuestas exitosas de los controladores y las envuelve automáticamente
/// en el formato estándar { success: true, data: ..., message: ... } para evitar código repetitivo.
/// </summary>
public class ApiResponseWrapperFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Si hay una excepción o no es un ObjectResult, no hacemos nada extra.
        // Las excepciones se manejan en ExceptionMiddleware.
        if (context.Result is not ObjectResult objectResult)
        {
            await next();
            return;
        }

        // Si el controlador ya devolvió explícitamente un ApiResponse (como en el controller de Booking),
        // no lo envolvemos dos veces.
        var isAlreadyWrapped = objectResult.Value != null && 
                               objectResult.Value.GetType().IsGenericType &&
                               (objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>) ||
                                objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(PagedApiResponse<>));

        if (isAlreadyWrapped)
        {
            await next();
            return;
        }

        // Solo envolvemos si es un código HTTP de éxito (2xx)
        if (objectResult.StatusCode >= 200 && objectResult.StatusCode < 300)
        {
            // Creamos un wrapper genérico ApiResponse<T>
            var valueType = objectResult.Value?.GetType() ?? typeof(object);
            
            // Caso especial: si es el tipo PagedResult genérico que usamos internamente, 
            // podríamos mapearlo, pero para hacerlo simple genéricamente:
            var genericResponseType = typeof(ApiResponse<>).MakeGenericType(valueType);
            
            var wrappedResponse = Activator.CreateInstance(genericResponseType);
            
            var successProp = genericResponseType.GetProperty("Success");
            var dataProp = genericResponseType.GetProperty("Data");
            
            successProp?.SetValue(wrappedResponse, true);
            dataProp?.SetValue(wrappedResponse, objectResult.Value);

            // Reemplazamos el valor original con el nuevo objeto envuelto
            objectResult.Value = wrappedResponse;
        }

        await next();
    }
}
