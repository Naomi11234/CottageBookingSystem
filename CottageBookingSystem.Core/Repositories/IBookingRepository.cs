
using CottageBookingSystem.Core.Entities;
using CottageBookingSystem.Core.Repositories;

namespace CottageBookingSystem.Core.Repositories;

public interface IBookingRepository : IRepository<CottageBooking>
{
    Task<PagedResult<CottageBooking>> GetPagedWithDetailsAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<List<CottageBooking>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> TryCreateIfAvailableAsync(
        CottageBooking booking,
        CancellationToken cancellationToken = default);
}
