using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    public partial class AddBronModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bronnen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContactmomentDetailsId = table.Column<string>(type: "text", nullable: false),
                    Soort = table.Column<string>(type: "text", nullable: false),
                    Titel = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bronnen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bronnen_ContactMomentDetails_ContactmomentDetailsId",
                        column: x => x.ContactmomentDetailsId,
                        principalTable: "ContactMomentDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bronnen_ContactmomentDetailsId",
                table: "Bronnen",
                column: "ContactmomentDetailsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bronnen");
        }
    }
}
