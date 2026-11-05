namespace CottageBookingSystem.Core.Entities;

public class CottageBooking
{
    public int Id { get; set; }
    public int CottageId { get; set; }
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int GuestsCount { get; set; } = 1;
    public string? Notes { get; set; }
    public string Status { get; set; } = "Pending";

    public Cottage Cottage { get; set; } = null!;
    public User User { get; set; } = null!;
}
