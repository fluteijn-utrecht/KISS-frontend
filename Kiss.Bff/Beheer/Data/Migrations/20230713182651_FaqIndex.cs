using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    public partial class FaqIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ContactmomentManagementLogs_PrimaireVraagWeergave",
                table: "ContactmomentManagementLogs",
                column: "PrimaireVraagWeergave");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContactmomentManagementLogs_PrimaireVraagWeergave",
                table: "ContactmomentManagementLogs");
        }
    }
}
