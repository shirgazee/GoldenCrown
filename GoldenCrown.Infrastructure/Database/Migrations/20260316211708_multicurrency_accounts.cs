using GoldenCrown.Domain.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldenCrown.Database.Migrations
{
    /// <inheritdoc />
    public partial class multicurrency_accounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_accounts_user_id",
                table: "accounts");

            migrationBuilder.AddColumn<string>(
                name: "currency",
                table: "accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_user_id",
                table: "accounts",
                column: "user_id");

            migrationBuilder.Sql(@$"
                insert into accounts (user_id, currency, balance)
                select u.id, '{Currency.EUR}', 0
                from users u;
");
            
            migrationBuilder.Sql(@$"
                insert into accounts (user_id, currency, balance)
                select u.id, '{Currency.GBP}', 0
                from users u;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_accounts_user_id",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "currency",
                table: "accounts");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_user_id",
                table: "accounts",
                column: "user_id",
                unique: true);
        }
    }
}
