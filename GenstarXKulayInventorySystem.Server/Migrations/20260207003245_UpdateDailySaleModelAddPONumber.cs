using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDailySaleModelAddPONumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedPaymentDate",
                table: "WayBills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "SaleItems",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateToBePaid",
                table: "DailySales",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PONumber",
                table: "DailySales",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedPaymentDate",
                table: "WayBills");

            migrationBuilder.DropColumn(
                name: "DateToBePaid",
                table: "DailySales");

            migrationBuilder.DropColumn(
                name: "PONumber",
                table: "DailySales");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "SaleItems",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");
        }
    }
}
