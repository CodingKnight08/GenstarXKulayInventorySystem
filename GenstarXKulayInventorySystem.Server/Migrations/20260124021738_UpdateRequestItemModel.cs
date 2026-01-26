using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRequestItemModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "RequestProductItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateRecieved",
                table: "RequestProductItems",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<decimal>(
                name: "ItemCost",
                table: "RequestProductItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ProductRequesterBranchId",
                table: "RequestProductItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductSourceBranchId",
                table: "RequestProductItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "RequestProductItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_RequestProductItems_ProductRequesterBranchId",
                table: "RequestProductItems",
                column: "ProductRequesterBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestProductItems_ProductSourceBranchId",
                table: "RequestProductItems",
                column: "ProductSourceBranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequestProductItems_BranchProducts_ProductRequesterBranchId",
                table: "RequestProductItems",
                column: "ProductRequesterBranchId",
                principalTable: "BranchProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestProductItems_BranchProducts_ProductSourceBranchId",
                table: "RequestProductItems",
                column: "ProductSourceBranchId",
                principalTable: "BranchProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestProductItems_BranchProducts_ProductRequesterBranchId",
                table: "RequestProductItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestProductItems_BranchProducts_ProductSourceBranchId",
                table: "RequestProductItems");

            migrationBuilder.DropIndex(
                name: "IX_RequestProductItems_ProductRequesterBranchId",
                table: "RequestProductItems");

            migrationBuilder.DropIndex(
                name: "IX_RequestProductItems_ProductSourceBranchId",
                table: "RequestProductItems");

            migrationBuilder.DropColumn(
                name: "ItemCost",
                table: "RequestProductItems");

            migrationBuilder.DropColumn(
                name: "ProductRequesterBranchId",
                table: "RequestProductItems");

            migrationBuilder.DropColumn(
                name: "ProductSourceBranchId",
                table: "RequestProductItems");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "RequestProductItems");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                table: "RequestProductItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateRecieved",
                table: "RequestProductItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
