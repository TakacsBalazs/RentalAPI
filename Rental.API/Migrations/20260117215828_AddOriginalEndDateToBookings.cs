using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rental.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalEndDateToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "OriginalEndDate",
                table: "Bookings",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalEndDate",
                table: "Bookings");
        }
    }
}
