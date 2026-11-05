using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CottageBookingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingGuestDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GuestsCount",
                table: "CottageBookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CottageBookings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CottageBookings_GuestsCount",
                table: "CottageBookings",
                sql: "[GuestsCount] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CottageBookings_GuestsCount",
                table: "CottageBookings");

            migrationBuilder.DropColumn(
                name: "GuestsCount",
                table: "CottageBookings");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CottageBookings");
        }
    }
}
