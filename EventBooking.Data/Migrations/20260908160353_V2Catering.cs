using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class V2Catering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CateringMenuId",
                table: "Bookings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CateringMenus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PricePerGuest = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    IsVegetarian = table.Column<bool>(type: "boolean", nullable: false),
                    IsVegan = table.Column<bool>(type: "boolean", nullable: false),
                    IncludesDrinks = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CateringMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CateringMenus_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CateringMenus",
                columns: new[] { "Id", "Description", "IncludesDrinks", "IsActive", "IsVegan", "IsVegetarian", "Name", "OwnerUserId", "PricePerGuest" },
                values: new object[,]
                {
                    { 1, "Full meat menu, first course to dessert.", true, true, false, false, "Meat Menu", new Guid("b0000000-0000-0000-0000-000000000002"), 220m },
                    { 2, "Dairy and fish menu.", true, true, false, true, "Dairy Menu", new Guid("b0000000-0000-0000-0000-000000000002"), 180m },
                    { 3, "Fully plant-based menu.", false, true, true, true, "Vegan Menu", new Guid("b0000000-0000-0000-0000-000000000002"), 160m }
                });

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "Price", "ServiceCategoryId" },
                values: new object[] { "Open bar, cocktails and soft drinks.", "Premium Bar Package", 90m, 8 });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CateringMenuId",
                table: "Bookings",
                column: "CateringMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_CateringMenus_OwnerUserId",
                table: "CateringMenus",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_CateringMenus_CateringMenuId",
                table: "Bookings",
                column: "CateringMenuId",
                principalTable: "CateringMenus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_CateringMenus_CateringMenuId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "CateringMenus");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CateringMenuId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CateringMenuId",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "Price", "ServiceCategoryId" },
                values: new object[] { "Full meat menu, first course to dessert.", "Catering - Meat Menu", 220m, 1 });
        }
    }
}
