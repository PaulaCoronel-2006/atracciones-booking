using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microservicios.Atracciones.Booking.Business.Interfaces;

namespace Microservicios.Atracciones.Booking.Business.Services;

public class LocalStorageService : IStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder = "uploads")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("El archivo es inválido o está vacío.", nameof(file));

        // 1. Asegurar que existe la ruta local wwwroot/folder
        var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var targetPath = Path.Combine(webRootPath, folder);

        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }

        // 2. Generar nombre de archivo único
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(targetPath, uniqueFileName);

        // 3. Guardar físicamente
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 4. Construir la URL pública base (ej: https://localhost:7123)
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
            throw new InvalidOperationException("No hay contexto HTTP disponible para construir la URL pública.");

        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

        // 5. Devolver la URL completa
        return $"{baseUrl}/{folder}/{uniqueFileName}";
    }
}
