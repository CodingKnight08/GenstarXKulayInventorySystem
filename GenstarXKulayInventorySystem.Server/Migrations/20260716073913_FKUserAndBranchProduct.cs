using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class FKUserAndBranchProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "BranchProducts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchProducts_UserId",
                table: "BranchProducts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchProducts_AspNetUsers_UserId",
                table: "BranchProducts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BranchProducts_AspNetUsers_UserId",
                table: "BranchProducts");

            migrationBuilder.DropIndex(
                name: "IX_BranchProducts_UserId",
                table: "BranchProducts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BranchProducts");
        }
    }
}
