using AutoMapper;
using CottageBookingSystem.Core.DTOs;
using CottageBookingSystem.Core.Entities;
using CottageBookingSystem.Core.Repositories;
using CottageBookingSystem.Core.Security;
using CottageBookingSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CottageBookingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class CottagesController(
    ICottageRepository cottageRepository,
    AppDbContext dbContext,
    IMapper mapper) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<CottageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CottageDto>>> GetCottages(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100)
        {
            return BadRequest(new { message = "pageNumber must be positive and pageSize must be between 1 and 100." });
        }

        var result = await cottageRepository.GetPagedWithAmenitiesAsync(
            pageNumber, pageSize, cancellationToken);

        return Ok(new PagedResult<CottageDto>(
            mapper.Map<IReadOnlyList<CottageDto>>(result.Items),
            result.PageNumber,
            result.PageSize,
            result.TotalCount));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CottageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CottageDto>> GetCottage(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var cottage = await cottageRepository.GetByIdWithAmenitiesAsync(
            id, cancellationToken);

        return cottage is null ? NotFound() : Ok(mapper.Map<CottageDto>(cottage));
    }

    [HttpGet("{id:int}/occupied-dates")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOccupiedDates(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var cottageExists = await dbContext.Cottages
            .AnyAsync(c => c.Id == id, cancellationToken);

        if (!cottageExists)
        {
            return NotFound(new { message = "Cottage was not found." });
        }

        var nowUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        var bookings = await dbContext.CottageBookings
            .AsNoTracking()
            .Where(b =>
                b.CottageId == id &&
                b.EndDate > nowUtc &&
                b.Status != "Cancelled" &&
                b.Status != "Canceled")
            .Select(b => new
            {
                startDate = b.StartDate,
                endDate = b.EndDate
            })
            .OrderBy(b => b.startDate)
            .ToListAsync(cancellationToken);

        return Ok(bookings);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(typeof(CottageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CottageDto>> CreateCottage(
        [FromBody] SaveCottageDto request,
        CancellationToken cancellationToken)
    {
        var cottage = new Cottage
        {
            Name = request.Name.Trim(),
            Location = request.Location.Trim(),
            PricePerNight = request.PricePerNight,
            MaxGuests = request.MaxGuests
        };

        var amenityIds = request.AmenityIds.Distinct().ToArray();
        var amenities = await dbContext.Amenities
            .Where(a => amenityIds.Contains(a.Id))
            .ToListAsync(cancellationToken);

        if (amenities.Count != amenityIds.Length)
        {
            return BadRequest(new { message = "One or more amenity IDs are invalid." });
        }

        foreach (var amenity in amenities)
        {
            cottage.Amenities.Add(amenity);
        }

        await cottageRepository.AddAsync(cottage, cancellationToken);
        await cottageRepository.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetCottage),
            new { id = cottage.Id },
            mapper.Map<CottageDto>(cottage));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCottage(
        int id,
        [FromBody] SaveCottageDto request,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var cottage = await dbContext.Cottages
            .Include(c => c.Amenities)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (cottage is null)
        {
            return NotFound();
        }

        var amenityIds = request.AmenityIds.Distinct().ToArray();
        var amenities = await dbContext.Amenities
            .Where(a => amenityIds.Contains(a.Id))
            .ToListAsync(cancellationToken);

        if (amenities.Count != amenityIds.Length)
        {
            return BadRequest(new { message = "One or more amenity IDs are invalid." });
        }

        cottage.Amenities.Clear();

        foreach (var amenity in amenities)
        {
            cottage.Amenities.Add(amenity);
        }

        cottage.Name = request.Name.Trim();
        cottage.Location = request.Location.Trim();
        cottage.PricePerNight = request.PricePerNight;
        cottage.MaxGuests = request.MaxGuests;

        await cottageRepository.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCottage(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var cottage = await cottageRepository.GetByIdAsync(id, cancellationToken);

        if (cottage is null)
        {
            return NotFound();
        }

        cottageRepository.Remove(cottage);

        try
        {
            await cottageRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "A cottage with existing bookings cannot be deleted." });
        }

        return NoContent();
    }
}