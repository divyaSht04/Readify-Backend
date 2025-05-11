using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PickupDate",
                table: "Bookings");

            migrationBuilder.AddColumn<string>(
                name: "image",
                table: "Books",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image",
                table: "Books");

            migrationBuilder.AddColumn<DateTime>(
                name: "PickupDate",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
