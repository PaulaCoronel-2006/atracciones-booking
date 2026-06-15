using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microservicios.Atracciones.Booking.Business.DTOs.Booking;

namespace Microservicios.Atracciones.Booking.API.Filters;

/// <summary>
/// Filtro de acción para manejar la idempotencia de solicitudes HTTP.
/// Requiere la cabecera 'X-Idempotency-Key' con un UUID válido.
/// </summary>
public class IdempotentFilter : IAsyncActionFilter
{
    private readonly IMemoryCache _cache;
    private const string HeaderName = "X-Idempotency-Key";

    public IdempotentFilter(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        // 1. Exigir la cabecera X-Idempotency-Key
        if (!request.Headers.TryGetValue(HeaderName, out var headerValue) || string.IsNullOrEmpty(headerValue))
        {
            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.Fail($"La cabecera '{HeaderName}' es obligatoria para este endpoint."));
            return;
        }

        // 2. Validar que sea un UUID válido
        if (!Guid.TryParse(headerValue, out var idempotencyKey))
        {
            context.Result = new BadRequestObjectResult(
                ApiResponse<object>.Fail($"El valor de la cabecera '{HeaderName}' debe ser un UUID válido."));
            return;
        }

        var cacheKey = $"idempotency:{idempotencyKey}";

        // 3. Verificar en caché
        if (_cache.TryGetValue(cacheKey, out var cachedValue))
        {
            if (cachedValue is string strValue)
            {
                if (strValue == "processing")
                {
                    // Conflicto de procesamiento concurrente
                    context.Result = new ConflictObjectResult(
                        ApiResponse<object>.Fail("Una solicitud idéntica ya está siendo procesada."));
                    return;
                }

                // Devolver respuesta cacheada
                context.HttpContext.Response.Headers.Append("X-Cache-Lookup", "HIT");
                
                try
                {
                    // Deserializamos y lo envolvemos en ContentResult
                    var contentResult = new ContentResult
                    {
                        Content = strValue,
                        ContentType = "application/json",
                        StatusCode = StatusCodes.Status200OK
                    };
                    context.Result = contentResult;
                    return;
                }
                catch
                {
                    // Si falla la deserialización, limpiamos y permitimos que continúe la ejecución
                    _cache.Remove(cacheKey);
                }
            }
        }

        // 4. Registrar como procesando
        _cache.Set(cacheKey, "processing", TimeSpan.FromMinutes(2));

        ActionExecutedContext executedContext;
        try
        {
            executedContext = await next();
        }
        catch (Exception)
        {
            _cache.Remove(cacheKey);
            throw;
        }

        // 5. Evaluar resultado de la ejecución
        if (executedContext.Exception != null)
        {
            _cache.Remove(cacheKey);
        }
        else
        {
            var statusCode = StatusCodes.Status200OK;
            if (executedContext.Result is ObjectResult objectResult)
            {
                statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            }
            else if (executedContext.Result is StatusCodeResult statusCodeResult)
            {
                statusCode = statusCodeResult.StatusCode;
            }

            if (statusCode >= 200 && statusCode < 300)
            {
                // Serializar respuesta exitosa
                object? responseValue = null;
                if (executedContext.Result is ObjectResult objResult)
                {
                    responseValue = objResult.Value;
                }

                var serializedResponse = JsonSerializer.Serialize(responseValue);
                _cache.Set(cacheKey, serializedResponse, TimeSpan.FromMinutes(2));
                executedContext.HttpContext.Response.Headers.Append("X-Cache-Lookup", "MISS");
            }
            else
            {
                // Remover clave si no fue exitosa (por ejemplo, BadRequest de validaciones)
                _cache.Remove(cacheKey);
            }
        }
    }
}
