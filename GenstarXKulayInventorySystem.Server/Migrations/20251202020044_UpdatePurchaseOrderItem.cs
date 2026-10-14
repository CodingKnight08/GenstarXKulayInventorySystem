using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePurchaseOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_Products_ProductId",
                table: "PurchaseOrderItems");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "PurchaseOrderItems",
                newName: "BranchProductId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderItems_ProductId",
                table: "PurchaseOrderItems",
                newName: "IX_PurchaseOrderItems_BranchProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_BranchProducts_BranchProductId",
                table: "PurchaseOrderItems",
                column: "BranchProductId",
                principalTable: "BranchProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_BranchProducts_BranchProductId",
                table: "PurchaseOrderItems");

            migrationBuilder.RenameColumn(
                name: "BranchProductId",
                table: "PurchaseOrderItems",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseOrderItems_BranchProductId",
                table: "PurchaseOrderItems",
                newName: "IX_PurchaseOrderItems_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_Products_ProductId",
                table: "PurchaseOrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
