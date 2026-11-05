using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CottageBookingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCottageSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CottageBookings_Cottages_CottageId",
                table: "CottageBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_CottageBookings_Users_UserId",
                table: "CottageBookings");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cottages_MaxGuests",
                table: "Cottages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cottages_PricePerNight",
                table: "Cottages");

            migrationBuilder.DropIndex(
                name: "IX_CottageBookings_CottageId_StartDate_EndDate",
                table: "CottageBookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CottageBookings_DateRange",
                table: "CottageBookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CottageBookings_GuestsCount",
                table: "CottageBookings");

            migrationBuilder.DeleteData(
                table: "CottageAmenities",
                keyColumns: new[] { "AmenityId", "CottageId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "CottageAmenities",
                keyColumns: new[] { "AmenityId", "CottageId" },
                keyValues: new object[] { 3, 1 });

            migrationBuilder.DeleteData(
                table: "CottageAmenities",
                keyColumns: new[] { "AmenityId", "CottageId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "CottageAmenities",
                keyColumns: new[] { "AmenityId", "CottageId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Customer");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(320)",
                oldMaxLength: 320);

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerNight",
                table: "Cottages",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Cottages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Cottages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "CottageBookings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "CottageBookings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Amenities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.UpdateData(
                table: "Cottages",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Location", "Name", "PricePerNight" },
                values: new object[] { "אמירים", "אחוזת העץ בערפל", 850m });

            migrationBuilder.UpdateData(
                table: "Cottages",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Location", "MaxGuests", "Name", "PricePerNight" },
                values: new object[] { "מג'דל שמס", 2, "בקתת האורנים בחרמון", 650m });

            migrationBuilder.InsertData(
                table: "Cottages",
                columns: new[] { "Id", "BookingVersion", "Location", "MaxGuests", "Name", "PricePerNight" },
                values: new object[] { 3, 0, "מצפה רמון", 5, "סוויטת נוף מדברי", 950m });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[] { 1, "naomi.avni21@gmail.com", "נעמי אבני", "Naomi216421370", "Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_CottageBookings_CottageId",
                table: "CottageBookings",
                column: "CottageId");

            migrationBuilder.AddForeignKey(
                name: "FK_CottageBookings_Cottages_CottageId",
                table: "CottageBookings",
                column: "CottageId",
                principalTable: "Cottages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CottageBookings_Users_UserId",
                table: "CottageBookings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CottageBookings_Cottages_CottageId",
                table: "CottageBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_CottageBookings_Users_UserId",
                table: "CottageBookings");

            migrationBuilder.DropIndex(
                name: "IX_CottageBookings_CottageId",
                table: "CottageBookings");

            migrationBuilder.DeleteData(
                table: "Cottages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Customer",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerNight",
                table: "Cottages",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Cottages",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Cottages",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "CottageBookings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "CottageBookings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Amenities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "Amenities",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Wi-Fi" },
                    { 2, "Pool" },
                    { 3, "Hot tub" }
                });

            migrationBuilder.UpdateData(
                table: "Cottages",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Location", "Name", "PricePerNight" },
                values: new object[] { "North", "Mountain Cottage", 450.00m });

            migrationBuilder.UpdateData(
                table: "Cottages",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Location", "MaxGuests", "Name", "PricePerNight" },
                values: new object[] { "Center", 3, "Garden Cottage", 380.00m });

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
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cottages_MaxGuests",
                table: "Cottages",
                sql: "[MaxGuests] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cottages_PricePerNight",
                table: "Cottages",
                sql: "[PricePerNight] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_CottageBookings_CottageId_StartDate_EndDate",
                table: "CottageBookings",
                columns: new[] { "CottageId", "StartDate", "EndDate" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_CottageBookings_DateRange",
                table: "CottageBookings",
                sql: "[StartDate] < [EndDate]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CottageBookings_GuestsCount",
                table: "CottageBookings",
                sql: "[GuestsCount] > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_CottageBookings_Cottages_CottageId",
                table: "CottageBookings",
                column: "CottageId",
                principalTable: "Cottages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CottageBookings_Users_UserId",
                table: "CottageBookings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}