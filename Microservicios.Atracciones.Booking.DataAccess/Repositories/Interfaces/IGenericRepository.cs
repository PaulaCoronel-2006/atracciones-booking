using Microservicios.Atracciones.Booking.DataAccess.Common;
using System.Linq.Expressions;

namespace Microservicios.Atracciones.Booking.DataAccess.Repositories.Interfaces;

public interface IGenericRepository<T> where T : class
{
    // Lectura
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<PagedResult<T>> GetPagedAsync(QueryFilters filters,
        Expression<Func<T, bool>>? filter = null);

    // Escritura
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Delete(T entity);           // Soft-delete si hereda de BaseEntity
    void DeleteRange(IEnumerable<T> entities);

    // Verificación
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    // Queryable crudo (para queries avanzadas)
    IQueryable<T> Query(bool asNoTracking = true);
}
