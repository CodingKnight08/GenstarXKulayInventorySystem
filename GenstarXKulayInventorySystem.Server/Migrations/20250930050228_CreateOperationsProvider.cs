using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GenstarXKulayInventorySystem.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreateOperationsProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OpertationsProviderId",
                table: "Billings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNumber",
                table: "Billings",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OperationsProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProviderName = table.Column<string>(type: "text", nullable: false),
                    TINNumber = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationsProviders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Billings_OpertationsProviderId",
                table: "Billings",
                column: "OpertationsProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Billings_OperationsProviders_OpertationsProviderId",
                table: "Billings",
                column: "OpertationsProviderId",
                principalTable: "OperationsProviders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Billings_OperationsProviders_OpertationsProviderId",
                table: "Billings");

            migrationBuilder.DropTable(
                name: "OperationsProviders");

            migrationBuilder.DropIndex(
                name: "IX_Billings_OpertationsProviderId",
                table: "Billings");

            migrationBuilder.DropColumn(
                name: "OpertationsProviderId",
                table: "Billings");

            migrationBuilder.DropColumn(
                name: "ReceiptNumber",
                table: "Billings");
        }
    }
}
