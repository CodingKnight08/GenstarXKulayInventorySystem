using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class WayBillDamageItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WayBillItems_WayBills_WayBillId",
                table: "WayBillItems");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "WayBills",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualQuantity",
                table: "WayBillItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsMergeToSystem",
                table: "WayBillItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "WayBillDamageItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WayBillItemId = table.Column<int>(type: "int", nullable: true),
                    DamageQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    DamageAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    TotalDamageCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WayBillDamageItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WayBillDamageItems_WayBillItems_WayBillItemId",
                        column: x => x.WayBillItemId,
                        principalTable: "WayBillItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WayBillDamageItems_WayBillItemId",
                table: "WayBillDamageItems",
                column: "WayBillItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_WayBillItems_WayBills_WayBillId",
                table: "WayBillItems",
                column: "WayBillId",
                principalTable: "WayBills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WayBillItems_WayBills_WayBillId",
                table: "WayBillItems");

            migrationBuilder.DropTable(
                name: "WayBillDamageItems");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "WayBills");

            migrationBuilder.DropColumn(
                name: "ActualQuantity",
                table: "WayBillItems");

            migrationBuilder.DropColumn(
                name: "IsMergeToSystem",
                table: "WayBillItems");

            migrationBuilder.AddForeignKey(
                name: "FK_WayBillItems_WayBills_WayBillId",
                table: "WayBillItems",
                column: "WayBillId",
                principalTable: "WayBills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
