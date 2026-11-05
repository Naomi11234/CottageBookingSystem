using CottageBookingSystem.Core.Security;

namespace CottageBookingSystem.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = RoleNames.Customer;

    public ICollection<CottageBooking> Bookings { get; set; } = new List<CottageBooking>();
}
