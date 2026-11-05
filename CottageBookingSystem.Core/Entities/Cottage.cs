namespace CottageBookingSystem.Core.Entities;

public class Cottage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
    public int BookingVersion { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public ICollection<CottageBooking> Bookings { get; set; } = new List<CottageBooking>();
    public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
}
