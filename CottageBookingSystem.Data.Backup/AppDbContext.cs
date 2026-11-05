
using CottageBookingSystem.Core.Entities;
using CottageBookingSystem.Core.Security;
using Microsoft.EntityFrameworkCore;

namespace CottageBookingSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cottage> Cottages => Set<Cottage>();
    public DbSet<User> Users => Set<User>();
    public DbSet<CottageBooking> CottageBookings => Set<CottageBooking>();
    public DbSet<Amenity> Amenities => Set<Amenity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map the existing many-to-many join table.
        modelBuilder.Entity<Cottage>()
            .HasMany(c => c.Amenities)
            .WithMany(a => a.Cottages)
            .UsingEntity<Dictionary<string, object>>(
                "CottageAmenities",
                right => right
                    .HasOne<Amenity>()
                    .WithMany()
                    .HasForeignKey("AmenityId"),
                left => left
                    .HasOne<Cottage>()
                    .WithMany()
                    .HasForeignKey("CottageId"),
                join =>
                {
                    join.ToTable("CottageAmenities");
                    join.HasKey("CottageId", "AmenityId");
                });

        // Existing administrator seed.
        // TODO: Replace plaintext password with a securely
        // generated password hash and update authentication.
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "נעמי אבני",
                Email = "naomi.avni21@gmail.com",
                PasswordHash = "Naomi216421370",
                Role = RoleNames.Admin
            }
        );

        // Existing cottage seed data.
        modelBuilder.Entity<Cottage>().HasData(
            new Cottage
            {
                Id = 1,
                Name = "אחוזת העץ בערפל",
                Location = "אמירים",
                PricePerNight = 850,
                MaxGuests = 4
            },
            new Cottage
            {
                Id = 2,
                Name = "בקתת האורנים בחרמון",
                Location = "מג'דל שמס",
                PricePerNight = 650,
                MaxGuests = 2
            },
            new Cottage
            {
                Id = 3,
                Name = "סוויטת נוף מדברי",
                Location = "מצפה רמון",
                PricePerNight = 950,
                MaxGuests = 5
            }
        );
    }
}
