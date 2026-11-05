using Microsoft.EntityFrameworkCore;
using CottageBookingSystem.Core.Repositories;

namespace CottageBookingSystem.Data.Repositories;

public class Repository<T>(AppDbContext dbContext) : IRepository<T> where T : class
{
    protected readonly AppDbContext DbContext = dbContext;
    private readonly DbSet<T> _dbSet = dbContext.Set<T>();

    protected static int GetSkipCount(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be at least 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");
        }

        return checked((pageNumber - 1) * pageSize);
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object?[] { id }, cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return DbContext.SaveChangesAsync(cancellationToken);
    }
}
