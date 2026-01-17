using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rental.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnDateToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "Bookings",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "Bookings");
        }
    }
}
