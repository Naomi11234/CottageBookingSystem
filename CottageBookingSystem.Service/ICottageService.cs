using CottageBookingSystem.Core.DTOs;
using CottageBookingSystem.Core.Repositories;

namespace CottageBookingSystem.Service;

public interface ICottageService
{
    Task<PagedResult<CottageDto>> GetCottagesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
