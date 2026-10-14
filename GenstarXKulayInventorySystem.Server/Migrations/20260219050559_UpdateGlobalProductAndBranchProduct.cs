using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGlobalProductAndBranchProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateToBePaid",
                table: "DailySales");

            migrationBuilder.AddColumn<bool>(
                name: "IsUrethane",
                table: "GlobalProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BasisProductId",
                table: "BranchProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TiedUpProductId",
                table: "BranchProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WholeSaleCostPrice",
                table: "BranchProducts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_BranchProducts_BasisProductId",
                table: "BranchProducts",
                column: "BasisProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchProducts_TiedUpProductId",
                table: "BranchProducts",
                column: "TiedUpProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchProducts_BranchProducts_BasisProductId",
                table: "BranchProducts",
                column: "BasisProductId",
                principalTable: "BranchProducts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchProducts_BranchProducts_TiedUpProductId",
                table: "BranchProducts",
                column: "TiedUpProductId",
                principalTable: "BranchProducts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BranchProducts_BranchProducts_BasisProductId",
                table: "BranchProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchProducts_BranchProducts_TiedUpProductId",
                table: "BranchProducts");

            migrationBuilder.DropIndex(
                name: "IX_BranchProducts_BasisProductId",
                table: "BranchProducts");

            migrationBuilder.DropIndex(
                name: "IX_BranchProducts_TiedUpProductId",
                table: "BranchProducts");

            migrationBuilder.DropColumn(
                name: "IsUrethane",
                table: "GlobalProducts");

            migrationBuilder.DropColumn(
                name: "BasisProductId",
                table: "BranchProducts");

            migrationBuilder.DropColumn(
                name: "TiedUpProductId",
                table: "BranchProducts");

            migrationBuilder.DropColumn(
                name: "WholeSaleCostPrice",
                table: "BranchProducts");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateToBePaid",
                table: "DailySales",
                type: "datetime2",
                nullable: true);
        }
    }
}
