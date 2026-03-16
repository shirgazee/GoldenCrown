using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldenCrown.Database.Migrations
{
    /// <inheritdoc />
    public partial class transaction_currency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "USD");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "currency",
                table: "transactions");
        }
    }
}
