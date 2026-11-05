namespace CottageBookingSystem.Core.DTOs;

public sealed class CottageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
    public IReadOnlyList<string> Amenities { get; set; } = Array.Empty<string>();
}
