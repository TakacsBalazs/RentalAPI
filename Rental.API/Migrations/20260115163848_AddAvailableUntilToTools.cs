using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rental.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailableUntilToTools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "AvailableUntil",
                table: "Tools",
                type: "date",
                nullable: false,
                defaultValueSql: "CAST(GETUTCDATE() AS DATE)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableUntil",
                table: "Tools");
        }
    }
}
