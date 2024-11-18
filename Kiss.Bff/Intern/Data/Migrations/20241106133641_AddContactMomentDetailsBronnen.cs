using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    public partial class AddContactMomentDetailsBronnen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactMomentDetailsBronnen",
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
                    table.PrimaryKey("PK_ContactMomentDetailsBronnen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactMomentDetailsBronnen_ContactMomentDetails_Contactmom~",
                        column: x => x.ContactmomentDetailsId,
                        principalTable: "ContactMomentDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactMomentDetailsBronnen_ContactmomentDetailsId",
                table: "ContactMomentDetailsBronnen",
                column: "ContactmomentDetailsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactMomentDetailsBronnen");
        }
    }
}
