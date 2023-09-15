using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedRolesToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1cbbb9b5-59ec-4c62-b7f8-468b7bef747f", "5ef1844b-6938-470d-a6a2-a92f2966bf36", "Administrator", "Administrator" },
                    { "e850a55d-adc7-48b2-a3c2-a89c0aed6caf", "91de3f1e-cc5d-4c62-a395-5880776ea9e7", "Manager", "Manager" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1cbbb9b5-59ec-4c62-b7f8-468b7bef747f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e850a55d-adc7-48b2-a3c2-a89c0aed6caf");
        }
    }
}
