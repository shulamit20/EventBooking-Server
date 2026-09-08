using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class V2Foundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventType",
                table: "Bookings");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "Venues",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ExtraServices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ExtraServices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "ExtraServices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Pricing",
                table: "ExtraServices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ServiceCategoryId",
                table: "ExtraServices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "HallSlotId",
                table: "Bookings",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Bookings",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LineTotal",
                table: "BookingExtraServices",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventTypeServiceCategories",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "integer", nullable: false),
                    ServiceCategoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypeServiceCategories", x => new { x.EventTypeId, x.ServiceCategoryId });
                    table.ForeignKey(
                        name: "FK_EventTypeServiceCategories_EventTypes_EventTypeId",
                        column: x => x.EventTypeId,
                        principalTable: "EventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTypeServiceCategories_ServiceCategories_ServiceCategor~",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EventTypes",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, null, true, "Wedding" },
                    { 2, null, true, "Bar Mitzvah" },
                    { 3, null, true, "Bat Mitzvah" },
                    { 4, null, true, "Corporate Event" },
                    { 5, null, true, "Birthday" },
                    { 6, null, true, "Private Event" },
                    { 7, null, true, "Other" }
                });

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ImageUrl", "IsActive", "OwnerUserId", "Pricing", "ServiceCategoryId", "UnitLabel" },
                values: new object[] { null, true, new Guid("b0000000-0000-0000-0000-000000000002"), "PerGuest", 1, "per guest" });

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ImageUrl", "IsActive", "OwnerUserId", "Pricing", "ServiceCategoryId" },
                values: new object[] { null, true, new Guid("b0000000-0000-0000-0000-000000000002"), "Flat", 6 });

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ImageUrl", "IsActive", "OwnerUserId", "Pricing", "ServiceCategoryId" },
                values: new object[] { null, true, new Guid("b0000000-0000-0000-0000-000000000002"), "Flat", 5 });

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ImageUrl", "IsActive", "OwnerUserId", "Pricing", "ServiceCategoryId" },
                values: new object[] { null, true, new Guid("b0000000-0000-0000-0000-000000000002"), "Flat", 4 });

            migrationBuilder.InsertData(
                table: "ServiceCategories",
                columns: new[] { "Id", "Code", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Catering", true, "Catering" },
                    { 2, "TableDesign", true, "Table Design" },
                    { 3, "BridalChair", true, "Bridal Chair" },
                    { 4, "Photography", true, "Photography" },
                    { 5, "DJ", true, "DJ / Music" },
                    { 6, "Flowers", true, "Flowers" },
                    { 7, "Lighting", true, "Lighting" },
                    { 8, "Other", true, "Other" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAtUtc", "DisplayName", "Email", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System Admin", "admin@eventbooking.local", "$2a$12$./ujlozjB7mpUpkNuPnKxOQEuUb/FVNPvZ1qYkSDWAD5Af8Sn0JtC", "Admin" },
                    { new Guid("b0000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Demo Manager", "manager@eventbooking.local", "$2a$12$./ujlozjB7mpUpkNuPnKxOQEuUb/FVNPvZ1qYkSDWAD5Af8Sn0JtC", "Manager" },
                    { new Guid("c0000000-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Demo Customer", "client@eventbooking.local", "$2a$12$./ujlozjB7mpUpkNuPnKxOQEuUb/FVNPvZ1qYkSDWAD5Af8Sn0JtC", "Customer" }
                });

            migrationBuilder.UpdateData(
                table: "Venues",
                keyColumn: "Id",
                keyValue: 1,
                column: "OwnerUserId",
                value: new Guid("b0000000-0000-0000-0000-000000000002"));

            migrationBuilder.UpdateData(
                table: "Venues",
                keyColumn: "Id",
                keyValue: 2,
                column: "OwnerUserId",
                value: new Guid("b0000000-0000-0000-0000-000000000002"));

            migrationBuilder.InsertData(
                table: "EventTypeServiceCategories",
                columns: new[] { "EventTypeId", "ServiceCategoryId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 3 },
                    { 1, 4 },
                    { 1, 5 },
                    { 1, 6 },
                    { 1, 7 },
                    { 1, 8 },
                    { 2, 1 },
                    { 2, 2 },
                    { 2, 4 },
                    { 2, 5 },
                    { 2, 6 },
                    { 2, 7 },
                    { 2, 8 },
                    { 3, 1 },
                    { 3, 2 },
                    { 3, 4 },
                    { 3, 5 },
                    { 3, 6 },
                    { 3, 7 },
                    { 3, 8 },
                    { 4, 1 },
                    { 4, 4 },
                    { 4, 5 },
                    { 4, 7 },
                    { 4, 8 },
                    { 5, 1 },
                    { 5, 2 },
                    { 5, 4 },
                    { 5, 5 },
                    { 5, 6 },
                    { 5, 8 },
                    { 6, 1 },
                    { 6, 2 },
                    { 6, 4 },
                    { 6, 5 },
                    { 6, 6 },
                    { 6, 7 },
                    { 6, 8 },
                    { 7, 1 },
                    { 7, 2 },
                    { 7, 4 },
                    { 7, 5 },
                    { 7, 6 },
                    { 7, 7 },
                    { 7, 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Venues_OwnerUserId",
                table: "Venues",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraServices_OwnerUserId",
                table: "ExtraServices",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtraServices_ServiceCategoryId",
                table: "ExtraServices",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EventTypeId",
                table: "Bookings",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTypes_Name",
                table: "EventTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTypeServiceCategories_ServiceCategoryId",
                table: "EventTypeServiceCategories",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_Code",
                table: "ServiceCategories",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_EventTypes_EventTypeId",
                table: "Bookings",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraServices_ServiceCategories_ServiceCategoryId",
                table: "ExtraServices",
                column: "ServiceCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExtraServices_Users_OwnerUserId",
                table: "ExtraServices",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_Users_OwnerUserId",
                table: "Venues",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_EventTypes_EventTypeId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ExtraServices_ServiceCategories_ServiceCategoryId",
                table: "ExtraServices");

            migrationBuilder.DropForeignKey(
                name: "FK_ExtraServices_Users_OwnerUserId",
                table: "ExtraServices");

            migrationBuilder.DropForeignKey(
                name: "FK_Venues_Users_OwnerUserId",
                table: "Venues");

            migrationBuilder.DropTable(
                name: "EventTypeServiceCategories");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropTable(
                name: "ServiceCategories");

            migrationBuilder.DropIndex(
                name: "IX_Venues_OwnerUserId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_ExtraServices_OwnerUserId",
                table: "ExtraServices");

            migrationBuilder.DropIndex(
                name: "IX_ExtraServices_ServiceCategoryId",
                table: "ExtraServices");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_EventTypeId",
                table: "Bookings");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ExtraServices");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ExtraServices");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "ExtraServices");

            migrationBuilder.DropColumn(
                name: "Pricing",
                table: "ExtraServices");

            migrationBuilder.DropColumn(
                name: "ServiceCategoryId",
                table: "ExtraServices");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LineTotal",
                table: "BookingExtraServices");

            migrationBuilder.AlterColumn<int>(
                name: "HallSlotId",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "Bookings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 1,
                column: "UnitLabel",
                value: "per plate");
        }
    }
}
