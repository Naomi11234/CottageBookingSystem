using System.ComponentModel.DataAnnotations;

namespace CottageBookingSystem.Core.DTOs;

public sealed class CreateBookingDto : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int CottageId { get; set; }

    [Required]          
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
    // הוספת השדות החסרים שמופיעים בשגיאה:
    public int GuestsCount { get; set; }
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.Date <= DateTime.UtcNow.Date)
        {
            yield return new ValidationResult("StartDate must be a future date (UTC).", [nameof(StartDate)]);
        }

        if (EndDate <= StartDate)
        {
            yield return new ValidationResult("EndDate must be later than StartDate.", [nameof(EndDate)]);
        }
    }
}
