using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    public partial class RemoveKlantContactemomentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KlantContactmomenten");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KlantContactmomenten",
                columns: table => new
                {
                    Klant = table.Column<string>(type: "text", nullable: false),
                    Contactmoment = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KlantContactmomenten", x => new { x.Klant, x.Contactmoment, x.Rol });
                });

            migrationBuilder.CreateIndex(
                name: "IX_KlantContactmomenten_Klant",
                table: "KlantContactmomenten",
                column: "Klant");
        }
    }
}
