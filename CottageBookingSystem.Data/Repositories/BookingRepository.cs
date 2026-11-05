using CottageBookingSystem.Core.Entities;
using CottageBookingSystem.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CottageBookingSystem.Data.Repositories;

public sealed class BookingRepository(AppDbContext dbContext)
    : Repository<CottageBooking>(dbContext), IBookingRepository
{
    public async Task<PagedResult<CottageBooking>> GetPagedWithDetailsAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var skip = GetSkipCount(pageNumber, pageSize);
        var query = DbContext.CottageBookings
            .AsNoTracking()
            .Include(booking => booking.Cottage)
                .ThenInclude(cottage => cottage.Amenities)
            .Include(booking => booking.User)
            .OrderBy(booking => booking.StartDate)
            .ThenBy(booking => booking.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var bookings = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CottageBooking>(bookings, pageNumber, pageSize, totalCount);
    }

    public async Task<bool> TryCreateIfAvailableAsync(
        CottageBooking booking,
        CancellationToken cancellationToken = default)
    {
        var cottage = await DbContext.Cottages
            .SingleOrDefaultAsync(candidate => candidate.Id == booking.CottageId, cancellationToken);

        if (cottage is null)
        {
            return false;
        }

        var hasConflict = await DbContext.CottageBookings.AnyAsync(existing =>
            existing.CottageId == booking.CottageId &&
            existing.StartDate < booking.EndDate &&
            booking.StartDate < existing.EndDate,
            cancellationToken);

        if (hasConflict)
        {
            return false;
        }

        cottage.BookingVersion++;
        await DbContext.CottageBookings.AddAsync(booking, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
