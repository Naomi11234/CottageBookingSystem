using CottageBookingSystem.Core.Entities;
using CottageBookingSystem.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CottageBookingSystem.Data.Repositories;

public sealed class CottageRepository(AppDbContext dbContext)
    : Repository<Cottage>(dbContext), ICottageRepository
{
    public async Task<PagedResult<Cottage>> GetPagedWithAmenitiesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var skip = GetSkipCount(pageNumber, pageSize);
        var query = DbContext.Cottages
            .AsNoTracking()
            .Include(cottage => cottage.Amenities)
            .OrderBy(cottage => cottage.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var cottages = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Cottage>(cottages, pageNumber, pageSize, totalCount);
    }

    public Task<Cottage?> GetByIdWithAmenitiesAsync(int id, CancellationToken cancellationToken = default)
    {
        return DbContext.Cottages
            .AsNoTracking()
            .Include(cottage => cottage.Amenities)
            .SingleOrDefaultAsync(cottage => cottage.Id == id, cancellationToken);
    }
}
