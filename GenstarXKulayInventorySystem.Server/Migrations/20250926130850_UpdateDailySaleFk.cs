using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDailySaleFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailySaleReportId",
                table: "DailySales",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailySales_DailySaleReportId",
                table: "DailySales",
                column: "DailySaleReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailySales_DailySaleReports_DailySaleReportId",
                table: "DailySales",
                column: "DailySaleReportId",
                principalTable: "DailySaleReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailySales_DailySaleReports_DailySaleReportId",
                table: "DailySales");

            migrationBuilder.DropIndex(
                name: "IX_DailySales_DailySaleReportId",
                table: "DailySales");

            migrationBuilder.DropColumn(
                name: "DailySaleReportId",
                table: "DailySales");
        }
    }
}
