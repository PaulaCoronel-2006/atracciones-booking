using Microsoft.AspNetCore.Http;

namespace Microservicios.Atracciones.Booking.Business.Interfaces;

/// <summary>
/// Interfaz para el guardado físico de archivos.
/// Permite intercambiar fácilmente entre almacenamiento local y en la nube (ej. Azure Blob Storage).
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Guarda un archivo físico y devuelve su URL pública.
    /// </summary>
    /// <param name="file">El archivo subido por el cliente.</param>
    /// <param name="folder">Carpeta destino opcional (ej: "attractions", "avatars").</param>
    /// <returns>La URL pública absoluta donde se puede acceder al archivo.</returns>
    Task<string> SaveFileAsync(IFormFile file, string folder = "uploads");
}
