using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSaleItemFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Clients_ClientId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_ClientId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "SaleItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "SaleItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_ClientId",
                table: "SaleItems",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_Clients_ClientId",
                table: "SaleItems",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
