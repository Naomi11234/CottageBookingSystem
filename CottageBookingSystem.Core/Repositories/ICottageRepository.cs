using CottageBookingSystem.Core.Entities;

namespace CottageBookingSystem.Core.Repositories;

public interface ICottageRepository : IRepository<Cottage>
{
    Task<PagedResult<Cottage>> GetPagedWithAmenitiesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Cottage?> GetByIdWithAmenitiesAsync(int id, CancellationToken cancellationToken = default);
}
