using System.ComponentModel.DataAnnotations;

namespace CottageBookingSystem.Core.DTOs;

public sealed class SaveCottageDto
{
    [Required, StringLength(150, MinimumLength = 1)]
    [RegularExpression(".*\\S.*", ErrorMessage = "Name cannot contain only whitespace.")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(250, MinimumLength = 1)]
    [RegularExpression(".*\\S.*", ErrorMessage = "Location cannot contain only whitespace.")]
    public string Location { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "99999999")]
    public decimal PricePerNight { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxGuests { get; set; }

    public List<int> AmenityIds { get; set; } = new();
}
