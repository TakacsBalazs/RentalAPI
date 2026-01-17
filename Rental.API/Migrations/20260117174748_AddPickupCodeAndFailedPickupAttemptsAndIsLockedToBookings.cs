using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rental.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPickupCodeAndFailedPickupAttemptsAndIsLockedToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedPickupAttempts",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PickupCode",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedPickupAttempts",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PickupCode",
                table: "Bookings");
        }
    }
}
