using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class WayBillModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WayBills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WayBillNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Courier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    DateReceived = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WayBills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WayBills_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WayBillItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WayBillId = table.Column<int>(type: "int", nullable: false),
                    BranchProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WayBillItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WayBillItems_BranchProducts_BranchProductId",
                        column: x => x.BranchProductId,
                        principalTable: "BranchProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WayBillItems_WayBills_WayBillId",
                        column: x => x.WayBillId,
                        principalTable: "WayBills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WayBillItems_BranchProductId",
                table: "WayBillItems",
                column: "BranchProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WayBillItems_WayBillId",
                table: "WayBillItems",
                column: "WayBillId");

            migrationBuilder.CreateIndex(
                name: "IX_WayBills_SupplierId",
                table: "WayBills",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WayBillItems");

            migrationBuilder.DropTable(
                name: "WayBills");
        }
    }
}
