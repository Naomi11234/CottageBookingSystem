namespace CottageBookingSystem.Core.DTOs;

public sealed class BookingResponseDto
{
    public int Id { get; set; }
    public int CottageId { get; set; }
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int GuestsCount { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
}
