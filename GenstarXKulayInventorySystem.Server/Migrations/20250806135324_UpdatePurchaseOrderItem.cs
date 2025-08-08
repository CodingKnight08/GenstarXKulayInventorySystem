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
            migrationBuilder.AddColumn<int>(
                name: "BillingId",
                table: "PurchaseOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_BillingId",
                table: "PurchaseOrderItems",
                column: "BillingId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_Billings_BillingId",
                table: "PurchaseOrderItems",
                column: "BillingId",
                principalTable: "Billings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_Billings_BillingId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_BillingId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "BillingId",
                table: "PurchaseOrderItems");
        }
    }
}
