using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    public partial class CmDetailsVerantwoordelijkeAfdeling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VerantwoordelijkeAfdeling",
                table: "ContactMomentDetails",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactMomentDetails_VerantwoordelijkeAfdeling",
                table: "ContactMomentDetails",
                column: "VerantwoordelijkeAfdeling");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContactMomentDetails_VerantwoordelijkeAfdeling",
                table: "ContactMomentDetails");

            migrationBuilder.DropColumn(
                name: "VerantwoordelijkeAfdeling",
                table: "ContactMomentDetails");
        }
    }
}
