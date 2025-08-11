using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFKBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billings_PurchaseOrders_PurchaseOrderId",
                table: "Billings");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_Billings_BillingId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Billings_PurchaseOrderId",
                table: "Billings");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderId",
                table: "Billings");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_Billings_BillingId",
                table: "PurchaseOrderItems",
                column: "BillingId",
                principalTable: "Billings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_Billings_BillingId",
                table: "PurchaseOrderItems");

            migrationBuilder.AddColumn<int>(
                name: "PurchaseOrderId",
                table: "Billings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Billings_PurchaseOrderId",
                table: "Billings",
                column: "PurchaseOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Billings_PurchaseOrders_PurchaseOrderId",
                table: "Billings",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_Billings_BillingId",
                table: "PurchaseOrderItems",
                column: "BillingId",
                principalTable: "Billings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
