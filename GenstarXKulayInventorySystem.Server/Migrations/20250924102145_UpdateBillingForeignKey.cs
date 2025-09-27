using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBillingForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "DailySaleId",
                table: "Billings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Billings_DailySaleId",
                table: "Billings",
                column: "DailySaleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Billings_DailySaleReports_DailySaleId",
                table: "Billings",
                column: "DailySaleId",
                principalTable: "DailySaleReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billings_DailySaleReports_DailySaleId",
                table: "Billings");

            migrationBuilder.DropIndex(
                name: "IX_Billings_DailySaleId",
                table: "Billings");

            migrationBuilder.DropColumn(
                name: "DailySaleId",
                table: "Billings");

            migrationBuilder.AddColumn<int>(
                name: "BillingId",
                table: "PurchaseOrderItems",
                type: "integer",
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
                principalColumn: "Id");
        }
    }
}
