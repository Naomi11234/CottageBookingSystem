
using CottageBookingSystem.Core.Entities;
using CottageBookingSystem.Core.Security;
using CottageBookingSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CottageBookingSystem.API;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var hasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher<User>>();

        var amenities = new[]
        {
          

    "Wi-Fi",
    "Pool",
    "Hot tub",
    "Parking",
    "Air conditioning",
    "Equipped kitchen",
    "Balcony",
    "BBQ",
    "Coffee machine",
    "TV",
    "Private yard",
    "Heated pool",
    "Sauna",
    "Children playground",
    "Underfloor heating",
    "Breakfast",
    "Sea view",
    "Mountain view",
    "Fireplace",
    "Outdoor seating area"
    };

        foreach (var name in amenities)
        {
            if (!await db.Amenities.AnyAsync(a => a.Name == name))
            {
                db.Amenities.Add(new Amenity { Name = name });
            }
        }

        await db.SaveChangesAsync();

        var amenityLookup = await db.Amenities
            .ToDictionaryAsync(a => a.Name, a => a);

        var cottages = new[]
        {
            new { Name = "Mountain View Cottage", Location = "Galilee", Price = 650m, Guests = 4, Amenities = new[] { "Wi-Fi", "Hot tub", "Parking" } },
            new { Name = "Garden Cottage", Location = "Center", Price = 480m, Guests = 3, Amenities = new[] { "Wi-Fi", "Air conditioning" } },
            new { Name = "Lake View Cottage", Location = "Golan Heights", Price = 850m, Guests = 6, Amenities = new[] { "Wi-Fi", "Pool", "Parking" } },
            new { Name = "Desert Retreat", Location = "Negev", Price = 550m, Guests = 4, Amenities = new[] { "Wi-Fi", "Air conditioning", "Parking" } },
            new { Name = "Forest Cabin", Location = "Carmel", Price = 720m, Guests = 5, Amenities = new[] { "Wi-Fi", "Hot tub", "Air conditioning" } }
        };

        foreach (var item in cottages)
        {
            if (await db.Cottages.AnyAsync(c => c.Name == item.Name))
                continue;

            var cottage = new Cottage
            {
                Name = item.Name,
                Location = item.Location,
                PricePerNight = item.Price,
                MaxGuests = item.Guests,
                BookingVersion = 0
            };

            foreach (var amenityName in item.Amenities)
                cottage.Amenities.Add(amenityLookup[amenityName]);

            db.Cottages.Add(cottage);
        }

        await db.SaveChangesAsync();

        var demoPassword = configuration["DemoSeed:Password"];

        if (string.IsNullOrWhiteSpace(demoPassword))
        {
            throw new InvalidOperationException(
                "Configure DemoSeed:Password in User Secrets.");
        }

        var customers = new[]
        {
            new { Name = "Demo Customer One", Email = "demo1@example.com" },
            new { Name = "Demo Customer Two", Email = "demo2@example.com" },
            new { Name = "Demo Customer Three", Email = "demo3@example.com" }
        };

        foreach (var item in customers)
        {
            var email = item.Email.ToUpperInvariant();

            if (await db.Users.AnyAsync(u => u.Email == email))
                continue;

            var user = new User
            {
                FullName = item.Name,
                Email = email,
                Role = RoleNames.Customer
            };

            user.PasswordHash =
                hasher.HashPassword(user, demoPassword);

            db.Users.Add(user);
        }

        await db.SaveChangesAsync();

        var start = DateTime.SpecifyKind(
            new DateTime(2026, 12, 1),
            DateTimeKind.Unspecified);

        var bookingData = new[]
        {
            new { Cottage = "Mountain View Cottage", Email = "demo1@example.com", Offset = 0, Nights = 3, Guests = 2, Status = "Confirmed" },
            new { Cottage = "Garden Cottage", Email = "demo2@example.com", Offset = 4, Nights = 2, Guests = 2, Status = "Pending" },
            new { Cottage = "Lake View Cottage", Email = "demo3@example.com", Offset = 7, Nights = 4, Guests = 5, Status = "Confirmed" },
            new { Cottage = "Desert Retreat", Email = "demo1@example.com", Offset = 12, Nights = 2, Guests = 3, Status = "Pending" },
            new { Cottage = "Forest Cabin", Email = "demo2@example.com", Offset = 16, Nights = 3, Guests = 4, Status = "Cancelled" }
        };

        foreach (var item in bookingData)
        {
            var cottage = await db.Cottages
                .SingleAsync(c => c.Name == item.Cottage);

            var user = await db.Users
                .SingleAsync(u => u.Email == item.Email.ToUpperInvariant());

            var bookingStart = start.AddDays(item.Offset);
            var bookingEnd = bookingStart.AddDays(item.Nights);

            var exists = await db.CottageBookings.AnyAsync(b =>
                b.CottageId == cottage.Id &&
                b.UserId == user.Id &&
                b.StartDate == bookingStart &&
                b.EndDate == bookingEnd);

            if (exists)
                continue;

            db.CottageBookings.Add(new CottageBooking
            {
                CottageId = cottage.Id,
                UserId = user.Id,
                StartDate = bookingStart,
                EndDate = bookingEnd,
                GuestsCount = item.Guests,
                Status = item.Status,
                Notes = "Demo booking"
            });
        }

        await db.SaveChangesAsync();
    }
}
