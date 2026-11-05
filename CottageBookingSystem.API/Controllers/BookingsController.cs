
using System.Security.Claims;
using CottageBookingSystem.Core.DTOs;
using CottageBookingSystem.Core.Repositories;
using CottageBookingSystem.Core.Security;
using CottageBookingSystem.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CottageBookingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.Customer + "," + RoleNames.Admin)]
[Produces("application/json")]
public sealed class BookingsController(
    IBookingService bookingService,
    ICottageRepository cottageRepository) : ControllerBase
{
    // מחזיר את כל ההזמנות של המשתמש המחובר
    [HttpGet("mine")]
    [ProducesResponseType(
        typeof(List<BookingResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<BookingResponseDto>>> GetMyBookings(
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var bookings = await bookingService.GetMyBookingsAsync(
            userId.Value,
            cancellationToken);

        return Ok(bookings);
    }

    // מחזיר הזמנה לפי מזהה
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(BookingResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponseDto>> GetBooking(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var booking = await bookingService.GetBookingByIdAsync(
            id,
            cancellationToken);

        if (booking is null)
        {
            return NotFound();
        }

        if (!IsAdmin() && booking.UserId != GetUserId())
        {
            return Forbid();
        }

        return Ok(booking);
    }

    // יצירת הזמנה חדשה
    [HttpPost]
    [ProducesResponseType(
        typeof(BookingResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponseDto>> CreateBooking(
        [FromBody] CreateBookingDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var cottage = await cottageRepository.GetByIdAsync(
            request.CottageId,
            cancellationToken);

        if (cottage is null)
        {
            return NotFound(new
            {
                message = "Cottage was not found."
            });
        }

        if (request.GuestsCount > cottage.MaxGuests)
        {
            return BadRequest(new
            {
                message =
                    $"GuestsCount cannot exceed this cottage's capacity of {cottage.MaxGuests}."
            });
        }

        var booking = await bookingService.TryCreateBookingAsync(
            request,
            userId.Value,
            cancellationToken);

        return booking is null
            ? Conflict(new
            {
                message = "The selected cottage is not available for these dates."
            })
            : CreatedAtAction(
                nameof(GetBooking),
                new { id = booking.Id },
                booking);
    }

    private bool IsAdmin()
    {
        return User.IsInRole(RoleNames.Admin);
    }

    private int? GetUserId()
    {
        return int.TryParse(
            User.FindFirstValue("UserId"),
            out var userId)
            ? userId
            : null;
    }
}
