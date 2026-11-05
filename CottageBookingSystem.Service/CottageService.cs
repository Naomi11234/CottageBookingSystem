using AutoMapper;
using CottageBookingSystem.Core.DTOs;
using CottageBookingSystem.Core.Repositories;

namespace CottageBookingSystem.Service;

public sealed class CottageService(ICottageRepository cottageRepository, IMapper mapper) : ICottageService
{
    public async Task<PagedResult<CottageDto>> GetCottagesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var page = await cottageRepository.GetPagedWithAmenitiesAsync(pageNumber, pageSize, cancellationToken);
        var cottages = mapper.Map<IReadOnlyList<CottageDto>>(page.Items);

        return new PagedResult<CottageDto>(cottages, page.PageNumber, page.PageSize, page.TotalCount);
    }
}
