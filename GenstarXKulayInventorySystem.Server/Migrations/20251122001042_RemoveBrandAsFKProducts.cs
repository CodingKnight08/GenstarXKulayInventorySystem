using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBrandAsFKProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GlobalProducts_ProductBrands_ProductBrandId",
                table: "GlobalProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductBrands_BrandId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_GlobalProducts_ProductBrandId",
                table: "GlobalProducts");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductBrandId",
                table: "GlobalProducts");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalProducts_BrandId",
                table: "GlobalProducts",
                column: "BrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_GlobalProducts_ProductBrands_BrandId",
                table: "GlobalProducts",
                column: "BrandId",
                principalTable: "ProductBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GlobalProducts_ProductBrands_BrandId",
                table: "GlobalProducts");

            migrationBuilder.DropIndex(
                name: "IX_GlobalProducts_BrandId",
                table: "GlobalProducts");

            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductBrandId",
                table: "GlobalProducts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalProducts_ProductBrandId",
                table: "GlobalProducts",
                column: "ProductBrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_GlobalProducts_ProductBrands_ProductBrandId",
                table: "GlobalProducts",
                column: "ProductBrandId",
                principalTable: "ProductBrands",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductBrands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "ProductBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
