using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class V2SlotGenerationAndHebrewSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EveningPrice",
                table: "Halls",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MorningPrice",
                table: "Halls",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NoonPrice",
                table: "Halls",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "CateringMenus",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "תפריט בשרי מלא, ממנה ראשונה עד קינוח.", "תפריט בשרי" });

            migrationBuilder.UpdateData(
                table: "CateringMenus",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "תפריט חלבי ודגים.", "תפריט חלבי" });

            migrationBuilder.UpdateData(
                table: "CateringMenus",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "תפריט צמחי מלא.", "תפריט טבעוני" });

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "חתונה");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "בר מצווה");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "בת מצווה");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "אירוע עסקי");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "יום הולדת");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "אירוע פרטי");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "אחר");

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "UnitLabel" },
                values: new object[] { "בר פתוח, קוקטיילים ומשקאות קלים.", "חבילת בר פרימיום", "לאורח" });

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "UnitLabel" },
                values: new object[] { "סידורי פרחים לשולחן", "לשולחן" });

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "UnitLabel" },
                values: new object[] { "להקה חיה", "לאירוע" });

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "UnitLabel" },
                values: new object[] { "צילום", "לאירוע" });

            migrationBuilder.UpdateData(
                table: "Halls",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EveningPrice", "MorningPrice", "Name", "NoonPrice" },
                values: new object[] { 15000m, 6000m, "האולם הראשי", 9000m });

            migrationBuilder.UpdateData(
                table: "Halls",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EveningPrice", "MorningPrice", "Name", "NoonPrice" },
                values: new object[] { 8000m, 4500m, "אולם הגן", 6000m });

            migrationBuilder.UpdateData(
                table: "Halls",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EveningPrice", "MorningPrice", "Name", "NoonPrice" },
                values: new object[] { 16000m, 6500m, "אולם הבדולח", 10000m });

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "קייטרינג");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "עיצוב שולחנות");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "כיסא כלה");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "צילום");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "תקליטן / מוזיקה");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "פרחים");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "תאורה");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "אחר");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"),
                column: "DisplayName",
                value: "מנהל מערכת");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000002"),
                column: "DisplayName",
                value: "מנהל דמו");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000003"),
                column: "DisplayName",
                value: "לקוח דמו");

            migrationBuilder.UpdateData(
                table: "Venues",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Address", "City", "Name" },
                values: new object[] { "רחוב מלכי ישראל 12", "ירושלים", "בית שמחה" });

            migrationBuilder.UpdateData(
                table: "Venues",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Address", "City", "Name" },
                values: new object[] { "רחוב רבי עקיבא 88", "בני ברק", "גני הפנינה" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EveningPrice",
                table: "Halls");

            migrationBuilder.DropColumn(
                name: "MorningPrice",
                table: "Halls");

            migrationBuilder.DropColumn(
                name: "NoonPrice",
                table: "Halls");

            migrationBuilder.UpdateData(
                table: "CateringMenus",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Full meat menu, first course to dessert.", "Meat Menu" });

            migrationBuilder.UpdateData(
                table: "CateringMenus",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Dairy and fish menu.", "Dairy Menu" });

            migrationBuilder.UpdateData(
                table: "CateringMenus",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Fully plant-based menu.", "Vegan Menu" });

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Wedding");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Bar Mitzvah");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Bat Mitzvah");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Corporate Event");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Birthday");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Private Event");

            migrationBuilder.UpdateData(
                table: "EventTypes",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Other");

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "UnitLabel" },
                values: new object[] { "Open bar, cocktails and soft drinks.", "Premium Bar Package", "per guest" });

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "UnitLabel" },
                values: new object[] { "Floral Centerpieces", "per table" });

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "UnitLabel" },
                values: new object[] { "Live Band", "per event" });

            migrationBuilder.UpdateData(
                table: "ExtraServices",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "UnitLabel" },
                values: new object[] { "Photography", "per event" });

            migrationBuilder.UpdateData(
                table: "Halls",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Main Ballroom");

            migrationBuilder.UpdateData(
                table: "Halls",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Garden Hall");

            migrationBuilder.UpdateData(
                table: "Halls",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Crystal Hall");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Catering");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Table Design");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Bridal Chair");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Photography");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "DJ / Music");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Flowers");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "Lighting");

            migrationBuilder.UpdateData(
                table: "ServiceCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Other");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"),
                column: "DisplayName",
                value: "System Admin");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0000-0000-0000-000000000002"),
                column: "DisplayName",
                value: "Demo Manager");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000003"),
                column: "DisplayName",
                value: "Demo Customer");

            migrationBuilder.UpdateData(
                table: "Venues",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Address", "City", "Name" },
                values: new object[] { "Rehov Malchei Yisrael 12", "Jerusalem", "Beit Simcha" });

            migrationBuilder.UpdateData(
                table: "Venues",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Address", "City", "Name" },
                values: new object[] { "Rehov Rabbi Akiva 88", "Bnei Brak", "Ganei HaPnina" });
        }
    }
}
