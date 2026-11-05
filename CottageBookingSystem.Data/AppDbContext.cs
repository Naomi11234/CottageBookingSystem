

using System;
using System.Collections.Generic;
using CottageBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CottageBookingSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options)
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

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(320);

            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Customer");

            entity.HasIndex(u => u.Email)
                .IsUnique();
        });

        modelBuilder.Entity<Cottage>(entity =>
        {
            entity.ToTable("Cottages", table =>
            {
                table.HasCheckConstraint(
                    "CK_Cottages_MaxGuests",
                    "[MaxGuests] > 0");

                table.HasCheckConstraint(
                    "CK_Cottages_PricePerNight",
                    "[PricePerNight] > 0");
            });

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(c => c.Location)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(c => c.PricePerNight)
                .HasPrecision(10, 2);

            entity.Property(c => c.RowVersion)
                .IsRowVersion();

            entity.HasData(
                new
                {
                    Id = 1,
                    Name = "Mountain Cottage",
                    Location = "North",
                    PricePerNight = 450.00m,
                    MaxGuests = 4,
                    BookingVersion = 0,
                    RowVersion = Array.Empty<byte>()
                },
                new
                {
                    Id = 2,
                    Name = "Garden Cottage",
                    Location = "Center",
                    PricePerNight = 380.00m,
                    MaxGuests = 3,
                    BookingVersion = 0,
                    RowVersion = Array.Empty<byte>()
                }
            );
        });

        modelBuilder.Entity<Amenity>(entity =>
        {
            entity.ToTable("Amenities");

            entity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasData(
                new { Id = 1, Name = "Wi-Fi" },
                new { Id = 2, Name = "Pool" },
                new { Id = 3, Name = "Hot tub" }
            );
        });

        modelBuilder.Entity<CottageBooking>(entity =>
        {
            entity.ToTable("CottageBookings", table =>
            {
                table.HasCheckConstraint(
                    "CK_CottageBookings_DateRange",
                    "[StartDate] < [EndDate]");

                table.HasCheckConstraint(
                    "CK_CottageBookings_GuestsCount",
                    "[GuestsCount] > 0");
            });

            entity.Property(b => b.Notes)
                .HasMaxLength(1000);

            entity.Property(b => b.Status)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Pending");

            entity.HasIndex(b => new
            {
                b.CottageId,
                b.StartDate,
                b.EndDate
            });

            entity.HasIndex(b => b.UserId);

            entity.HasOne(b => b.Cottage)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CottageId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cottage>()
            .HasMany(c => c.Amenities)
            .WithMany(a => a.Cottages)
            .UsingEntity<Dictionary<string, object>>(
                "CottageAmenity",
                right => right
                    .HasOne<Amenity>()
                    .WithMany()
                    .HasForeignKey("AmenityId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Cottage>()
                    .WithMany()
                    .HasForeignKey("CottageId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("CottageAmenities");
                    join.HasKey("CottageId", "AmenityId");
                    join.HasIndex("AmenityId");

                    join.HasData(
                        new { CottageId = 1, AmenityId = 1 },
                        new { CottageId = 1, AmenityId = 3 },
                        new { CottageId = 2, AmenityId = 1 },
                        new { CottageId = 2, AmenityId = 2 }
                    );
                });
    }
}
