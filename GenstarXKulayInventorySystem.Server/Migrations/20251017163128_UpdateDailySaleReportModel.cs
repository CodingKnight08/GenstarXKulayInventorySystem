using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDailySaleReportModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GrossProfit",
                table: "DailySaleReports",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LandedCost",
                table: "DailySaleReports",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrossProfit",
                table: "DailySaleReports");

            migrationBuilder.DropColumn(
                name: "LandedCost",
                table: "DailySaleReports");
        }
    }
}
