using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingBalanceFieldClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnItems_BranchProducts_BranchProductId",
                table: "ReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnItems_DailySales_DailySaleId",
                table: "ReturnItems");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "ReturnItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ReturnItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingChargeBalance",
                table: "Clients",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnItems_BranchProducts_BranchProductId",
                table: "ReturnItems",
                column: "BranchProductId",
                principalTable: "BranchProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnItems_DailySales_DailySaleId",
                table: "ReturnItems",
                column: "DailySaleId",
                principalTable: "DailySales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReturnItems_BranchProducts_BranchProductId",
                table: "ReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnItems_DailySales_DailySaleId",
                table: "ReturnItems");

            migrationBuilder.DropColumn(
                name: "RemainingChargeBalance",
                table: "Clients");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "ReturnItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ReturnItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnItems_BranchProducts_BranchProductId",
                table: "ReturnItems",
                column: "BranchProductId",
                principalTable: "BranchProducts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnItems_DailySales_DailySaleId",
                table: "ReturnItems",
                column: "DailySaleId",
                principalTable: "DailySales",
                principalColumn: "Id");
        }
    }
}
