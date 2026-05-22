using System.Net;
using System.Text.Json;
using Microservicios.Atracciones.Booking.Business.Exceptions;

namespace Microservicios.Atracciones.Booking.API.Middleware;

/// <summary>
/// Intercepta excepciones de negocio (BusinessException, NotFoundException, etc) y las transforma
/// en respuestas HTTP estandarizadas para el cliente, evitando filtración de StackTraces.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error inesperado al procesar la solicitud.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new ExceptionResponse();

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = validationEx.Message;
                response.Errors = validationEx.Errors;
                break;
                
            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = notFoundEx.Message;
                break;

            case UnauthorizedBusinessException unauthEx:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Message = unauthEx.Message;
                break;

            case BusinessException businessEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = businessEx.Message;
                break;

            case ConflictException conflictEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Message = conflictEx.Message;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = exception.InnerException?.Message ?? exception.Message;
                break;
        }

        var result = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return context.Response.WriteAsync(result);
    }
}

public class ExceptionResponse
{
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
}
