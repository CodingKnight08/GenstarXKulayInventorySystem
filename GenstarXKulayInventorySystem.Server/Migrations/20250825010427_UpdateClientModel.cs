using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClientModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailySales_Clients_ClientId",
                table: "DailySales");

            migrationBuilder.AddColumn<int>(
                name: "Branch",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_DailySales_Clients_ClientId",
                table: "DailySales",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailySales_Clients_ClientId",
                table: "DailySales");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Clients");

            migrationBuilder.AddForeignKey(
                name: "FK_DailySales_Clients_ClientId",
                table: "DailySales",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }
    }
}
