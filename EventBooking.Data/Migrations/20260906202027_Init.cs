using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExtraServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    UnitLabel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtraServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Halls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VenueId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Halls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Halls_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HallSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HallId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Shift = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BasePrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HallSlots_Halls_HallId",
                        column: x => x.HallId,
                        principalTable: "Halls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HallSlotId = table.Column<int>(type: "integer", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HostName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GuestCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_HallSlots_HallSlotId",
                        column: x => x.HallSlotId,
                        principalTable: "HallSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingExtraServices",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    ExtraServiceId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    PriceAtBooking = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingExtraServices", x => new { x.BookingId, x.ExtraServiceId });
                    table.ForeignKey(
                        name: "FK_BookingExtraServices_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingExtraServices_ExtraServices_ExtraServiceId",
                        column: x => x.ExtraServiceId,
                        principalTable: "ExtraServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ExtraServices",
                columns: new[] { "Id", "Description", "Name", "Price", "UnitLabel" },
                values: new object[,]
                {
                    { 1, "Full meat menu, first course to dessert.", "Catering - Meat Menu", 220m, "per plate" },
                    { 2, null, "Floral Centerpieces", 180m, "per table" },
                    { 3, null, "Live Band", 8000m, "per event" },
                    { 4, null, "Photography", 5000m, "per event" }
                });

            migrationBuilder.InsertData(
                table: "Venues",
                columns: new[] { "Id", "Address", "City", "ContactPhone", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Rehov Malchei Yisrael 12", "Jerusalem", "02-500-1000", null, "Beit Simcha" },
                    { 2, "Rehov Rabbi Akiva 88", "Bnei Brak", "03-570-2000", null, "Ganei HaPnina" }
                });

            migrationBuilder.InsertData(
                table: "Halls",
                columns: new[] { "Id", "Capacity", "Description", "Name", "VenueId" },
                values: new object[,]
                {
                    { 1, 400, null, "Main Ballroom", 1 },
                    { 2, 150, null, "Garden Hall", 1 },
                    { 3, 300, null, "Crystal Hall", 2 }
                });

            migrationBuilder.InsertData(
                table: "HallSlots",
                columns: new[] { "Id", "BasePrice", "Date", "HallId", "Shift", "Status", "Version" },
                values: new object[,]
                {
                    { 1, 6000m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Morning", "Available", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 2, 9000m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Noon", "Available", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 3, 15000m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Evening", "Available", new Guid("33333333-3333-3333-3333-333333333333") },
                    { 4, 15000m, new DateTime(2026, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Evening", "Available", new Guid("44444444-4444-4444-4444-444444444444") },
                    { 5, 8000m, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "Evening", "Available", new Guid("55555555-5555-5555-5555-555555555555") },
                    { 6, 10000m, new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Noon", "Available", new Guid("66666666-6666-6666-6666-666666666666") },
                    { 7, 16000m, new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Evening", "Available", new Guid("77777777-7777-7777-7777-777777777777") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingExtraServices_ExtraServiceId",
                table: "BookingExtraServices",
                column: "ExtraServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HallSlotId_Status",
                table: "Bookings",
                columns: new[] { "HallSlotId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_OwnerUserId",
                table: "Bookings",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Halls_VenueId_Name",
                table: "Halls",
                columns: new[] { "VenueId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HallSlots_HallId_Date_Shift",
                table: "HallSlots",
                columns: new[] { "HallId", "Date", "Shift" },
                unique: true);

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
                name: "BookingExtraServices");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "ExtraServices");

            migrationBuilder.DropTable(
                name: "HallSlots");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Halls");

            migrationBuilder.DropTable(
                name: "Venues");
        }
    }
}
