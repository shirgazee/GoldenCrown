using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GoldenCrown.Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedUserData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "login", "name", "password" },
                values: new object[,]
                {
                    { 1, "admin", "Administrator", "admin" },
                    { 2, "user", "Regular User", "user" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: 2);
        }
    }
}
