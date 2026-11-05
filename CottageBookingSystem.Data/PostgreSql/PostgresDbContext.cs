
using Microsoft.EntityFrameworkCore;

namespace CottageBookingSystem.Data.PostgreSql;

public class PostgresDbContext : AppDbContext
{
    public PostgresDbContext(
        DbContextOptions<PostgresDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Core.Entities.CottageBooking>(entity =>
        {
            entity.Property(b => b.StartDate)
                .HasColumnType("timestamp without time zone");

            entity.Property(b => b.EndDate)
                .HasColumnType("timestamp without time zone");
        });
        modelBuilder.Entity<Core.Entities.Cottage>(entity =>
        {
            entity.Ignore(c => c.RowVersion);

            entity.Property(c => c.BookingVersion)
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<Core.Entities.Cottage>()
            .ToTable("Cottages", table =>
            {
                table.HasCheckConstraint(
                    "CK_Cottages_MaxGuests",
                    "\"MaxGuests\" > 0");

                table.HasCheckConstraint(
                    "CK_Cottages_PricePerNight",
                    "\"PricePerNight\" > 0");
            });

        modelBuilder.Entity<Core.Entities.CottageBooking>()
            .ToTable("CottageBookings", table =>
            {
                table.HasCheckConstraint(
                    "CK_CottageBookings_DateRange",
                    "\"StartDate\" < \"EndDate\"");

                table.HasCheckConstraint(
                    "CK_CottageBookings_GuestsCount",
                    "\"GuestsCount\" > 0");
            });
    }
}
