using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class FixBillingFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billings_OperationsProviders_OpertationsProviderId",
                table: "Billings");

            migrationBuilder.RenameColumn(
                name: "OpertationsProviderId",
                table: "Billings",
                newName: "OperationsProviderId");

            migrationBuilder.RenameIndex(
                name: "IX_Billings_OpertationsProviderId",
                table: "Billings",
                newName: "IX_Billings_OperationsProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Billings_OperationsProviders_OperationsProviderId",
                table: "Billings",
                column: "OperationsProviderId",
                principalTable: "OperationsProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billings_OperationsProviders_OperationsProviderId",
                table: "Billings");

            migrationBuilder.RenameColumn(
                name: "OperationsProviderId",
                table: "Billings",
                newName: "OpertationsProviderId");

            migrationBuilder.RenameIndex(
                name: "IX_Billings_OperationsProviderId",
                table: "Billings",
                newName: "IX_Billings_OpertationsProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Billings_OperationsProviders_OpertationsProviderId",
                table: "Billings",
                column: "OpertationsProviderId",
                principalTable: "OperationsProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
