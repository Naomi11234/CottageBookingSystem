namespace CottageBookingSystem.Core.Entities;

public class Amenity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Cottage> Cottages { get; set; } = new List<Cottage>();
}
