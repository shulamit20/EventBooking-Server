using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingOwnerCreatedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_OwnerUserId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_OwnerUserId_CreatedAtUtc",
                table: "Bookings",
                columns: new[] { "OwnerUserId", "CreatedAtUtc" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_OwnerUserId_CreatedAtUtc",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_OwnerUserId",
                table: "Bookings",
                column: "OwnerUserId");
        }
    }
}
