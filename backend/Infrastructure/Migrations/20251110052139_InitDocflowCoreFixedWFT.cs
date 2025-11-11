using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitDocflowCoreFixedWFT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WFTrackers_Documents_DocumentId",
                table: "WFTrackers");

            migrationBuilder.AddForeignKey(
                name: "FK_WFTrackers_Documents_DocumentId",
                table: "WFTrackers",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WFTrackers_Documents_DocumentId",
                table: "WFTrackers");

            migrationBuilder.AddForeignKey(
                name: "FK_WFTrackers_Documents_DocumentId",
                table: "WFTrackers",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
