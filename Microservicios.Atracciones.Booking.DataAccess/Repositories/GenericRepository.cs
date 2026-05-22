using Microsoft.EntityFrameworkCore;
using Microservicios.Atracciones.Booking.DataAccess.Common;
using Microservicios.Atracciones.Booking.DataAccess.Context;
using Microservicios.Atracciones.Booking.DataAccess.Repositories.Interfaces;
using System.Linq.Expressions;

namespace Microservicios.Atracciones.Booking.DataAccess.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AtraccionDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AtraccionDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<PagedResult<T>> GetPagedAsync(QueryFilters filters, Expression<Func<T, bool>>? filter = null)
    {
        var query = _dbSet.AsQueryable();

        if (filter != null)
        {
            query = query.Where(filter);
        }

        var totalCount = await query.CountAsync();
        
        var items = await query.Skip(filters.Offset)
                               .Take(filters.PageSize)
                               .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public void Update(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate != null)
        {
            return await _dbSet.CountAsync(predicate);
        }
        return await _dbSet.CountAsync();
    }

    public IQueryable<T> Query(bool asNoTracking = true)
    {
        var query = _dbSet.AsQueryable();
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }
        return query;
    }
}
