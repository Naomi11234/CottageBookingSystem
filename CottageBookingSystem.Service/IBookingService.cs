using CottageBookingSystem.Core.DTOs;

namespace CottageBookingSystem.Service;

public interface IBookingService
{
    Task<BookingResponseDto?> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default);

    // Returns null when the repository finds that another booking already overlaps this date range.
    Task<BookingResponseDto?> TryCreateBookingAsync(
        CreateBookingDto request,
        int userId,
        CancellationToken cancellationToken = default);
}
