
using CottageBookingSystem.Core.DTOs;

namespace CottageBookingSystem.Service;

public interface IBookingService
{
    Task<BookingResponseDto?> GetBookingByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<List<BookingResponseDto>> GetMyBookingsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<BookingResponseDto?> TryCreateBookingAsync(
        CreateBookingDto request,
        int userId,
        CancellationToken cancellationToken = default);
}
