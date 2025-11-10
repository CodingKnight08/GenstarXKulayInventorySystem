using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFkReturnSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnSale_DailySales_SalesNumber",
                table: "ReturnSale");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReturnSale",
                table: "ReturnSale");

            migrationBuilder.RenameTable(
                name: "ReturnSale",
                newName: "ReturnSales");

            migrationBuilder.RenameIndex(
                name: "IX_ReturnSale_SalesNumber",
                table: "ReturnSales",
                newName: "IX_ReturnSales_SalesNumber");

            migrationBuilder.AddColumn<int>(
                name: "Branch",
                table: "ReturnSales",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ReturnSales",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReturnSales",
                table: "ReturnSales",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnSales_ProductId",
                table: "ReturnSales",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnSales_DailySales_SalesNumber",
                table: "ReturnSales",
                column: "SalesNumber",
                principalTable: "DailySales",
                principalColumn: "SalesNumber",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnSales_Products_ProductId",
                table: "ReturnSales",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnSales_DailySales_SalesNumber",
                table: "ReturnSales");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnSales_Products_ProductId",
                table: "ReturnSales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReturnSales",
                table: "ReturnSales");

            migrationBuilder.DropIndex(
                name: "IX_ReturnSales_ProductId",
                table: "ReturnSales");

            migrationBuilder.DropColumn(
                name: "Branch",
                table: "ReturnSales");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ReturnSales");

            migrationBuilder.RenameTable(
                name: "ReturnSales",
                newName: "ReturnSale");

            migrationBuilder.RenameIndex(
                name: "IX_ReturnSales_SalesNumber",
                table: "ReturnSale",
                newName: "IX_ReturnSale_SalesNumber");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReturnSale",
                table: "ReturnSale",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnSale_DailySales_SalesNumber",
                table: "ReturnSale",
                column: "SalesNumber",
                principalTable: "DailySales",
                principalColumn: "SalesNumber",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
