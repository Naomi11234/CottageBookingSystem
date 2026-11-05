using System.ComponentModel.DataAnnotations;

namespace CottageBookingSystem.Core.DTOs;

public sealed class RegisterRequestDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    [RegularExpression(".*\\S.*", ErrorMessage = "FullName cannot contain only whitespace.")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12)]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{12,128}$",
        ErrorMessage = "Password must include uppercase, lowercase, a digit, and a symbol.")]
    public string Password { get; set; } = string.Empty;
}
