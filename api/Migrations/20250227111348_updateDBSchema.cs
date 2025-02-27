using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class updateDBSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5d94d47f-6b3f-4e10-ab1e-24a7624d6aed");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e468a9d0-5c20-44d7-b41b-b7bc566993e3");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1a2b3c4d-0000-1111-2222-3e4f5g6h7i8j", null, "Admin", "ADMIN" },
                    { "2b3c4d5e-0000-1111-2222-3e4f5g6h7i8j", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1a2b3c4d-0000-1111-2222-3e4f5g6h7i8j");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2b3c4d5e-0000-1111-2222-3e4f5g6h7i8j");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5d94d47f-6b3f-4e10-ab1e-24a7624d6aed", null, "User", "USER" },
                    { "e468a9d0-5c20-44d7-b41b-b7bc566993e3", null, "Admin", "ADMIN" }
                });
        }
    }
}
