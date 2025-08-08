using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddDataFieldBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "Billings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "Billings");
        }
    }
}
