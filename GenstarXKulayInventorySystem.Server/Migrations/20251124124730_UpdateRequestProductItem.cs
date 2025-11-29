using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRequestProductItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestProductItems_Products_ProductId",
                table: "RequestProductItems");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "RequestProductItems",
                newName: "MasterProductId");

            migrationBuilder.RenameIndex(
                name: "IX_RequestProductItems_ProductId",
                table: "RequestProductItems",
                newName: "IX_RequestProductItems_MasterProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestProductItems_GlobalProducts_MasterProductId",
                table: "RequestProductItems",
                column: "MasterProductId",
                principalTable: "GlobalProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestProductItems_GlobalProducts_MasterProductId",
                table: "RequestProductItems");

            migrationBuilder.RenameColumn(
                name: "MasterProductId",
                table: "RequestProductItems",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_RequestProductItems_MasterProductId",
                table: "RequestProductItems",
                newName: "IX_RequestProductItems_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestProductItems_Products_ProductId",
                table: "RequestProductItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
