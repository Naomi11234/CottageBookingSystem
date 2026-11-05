

using AutoMapper;
using CottageBookingSystem.Core.DTOs;
using CottageBookingSystem.Core.Entities;
using CottageBookingSystem.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CottageBookingSystem.Service;

public sealed class BookingService(
    IBookingRepository bookingRepository,
    IMapper mapper,
    ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingResponseDto?> GetBookingByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "Booking ID must be greater than zero.");
        }

        var booking = await bookingRepository.GetByIdAsync(
            id,
            cancellationToken);

        return booking is null
            ? null
            : mapper.Map<BookingResponseDto>(booking);
    }

    // מחזיר את ההזמנות של המשתמש המחובר
    public async Task<List<BookingResponseDto>> GetMyBookingsAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId));
        }

        var bookings = await bookingRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        return mapper.Map<List<BookingResponseDto>>(bookings);
    }

    public async Task<BookingResponseDto?> TryCreateBookingAsync(
        CreateBookingDto request,
        int userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CottageId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.CottageId),
                "CottageId must be greater than zero.");
        }

        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(userId),
                "UserId must be greater than zero.");
        }

        if (request.StartDate >= request.EndDate)
        {
            throw new ArgumentException(
                "EndDate must be later than StartDate.",
                nameof(request));
        }

        if (request.StartDate.Date <= DateTime.UtcNow.Date)
        {
            throw new ArgumentException(
                "StartDate must be a future date (UTC).",
                nameof(request));
        }

        var booking = mapper.Map<CottageBooking>(request);

        booking.UserId = userId;
        booking.Status = "Pending";

        try
        {
            var created = await bookingRepository.TryCreateIfAvailableAsync(
                booking,
                cancellationToken);

            return created
                ? mapper.Map<BookingResponseDto>(booking)
                : null;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogWarning(
                exception,
                "Concurrent booking attempt detected for cottage {CottageId} by user {UserId}.",
                request.CottageId,
                userId);

            throw new BookingConflictException(exception);
        }
    }
}
