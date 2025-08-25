using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDailySaleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "DailySales",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailySales_ClientId",
                table: "DailySales",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailySales_Clients_ClientId",
                table: "DailySales",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailySales_Clients_ClientId",
                table: "DailySales");

            migrationBuilder.DropIndex(
                name: "IX_DailySales_ClientId",
                table: "DailySales");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "DailySales");
        }
    }
}
