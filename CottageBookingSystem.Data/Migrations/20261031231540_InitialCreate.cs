using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CottageBookingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cottages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PricePerNight = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxGuests = table.Column<int>(type: "int", nullable: false),
                    BookingVersion = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cottages", x => x.Id);
                    table.CheckConstraint("CK_Cottages_MaxGuests", "[MaxGuests] > 0");
                    table.CheckConstraint("CK_Cottages_PricePerNight", "[PricePerNight] > 0");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Customer")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CottageAmenities",
                columns: table => new
                {
                    CottageId = table.Column<int>(type: "int", nullable: false),
                    AmenityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CottageAmenities", x => new { x.CottageId, x.AmenityId });
                    table.ForeignKey(
                        name: "FK_CottageAmenities_Amenities_AmenityId",
                        column: x => x.AmenityId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CottageAmenities_Cottages_CottageId",
                        column: x => x.CottageId,
                        principalTable: "Cottages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CottageBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CottageId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CottageBookings", x => x.Id);
                    table.CheckConstraint("CK_CottageBookings_DateRange", "[StartDate] < [EndDate]");
                    table.ForeignKey(
                        name: "FK_CottageBookings_Cottages_CottageId",
                        column: x => x.CottageId,
                        principalTable: "Cottages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CottageBookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Amenities",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Wi-Fi" },
                    { 2, "Pool" },
                    { 3, "Hot tub" }
                });

            migrationBuilder.InsertData(
                table: "Cottages",
                columns: new[] { "Id", "BookingVersion", "Location", "MaxGuests", "Name", "PricePerNight" },
                values: new object[,]
                {
                    { 1, 0, "North", 4, "Mountain Cottage", 450.00m },
                    { 2, 0, "Center", 3, "Garden Cottage", 380.00m }
                });

            migrationBuilder.InsertData(
                table: "CottageAmenities",
                columns: new[] { "AmenityId", "CottageId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 3, 1 },
                    { 1, 2 },
                    { 2, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CottageAmenities_AmenityId",
                table: "CottageAmenities",
                column: "AmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_CottageBookings_CottageId_StartDate_EndDate",
                table: "CottageBookings",
                columns: new[] { "CottageId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CottageBookings_UserId",
                table: "CottageBookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CottageAmenities");

            migrationBuilder.DropTable(
                name: "CottageBookings");

            migrationBuilder.DropTable(
                name: "Amenities");

            migrationBuilder.DropTable(
                name: "Cottages");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
