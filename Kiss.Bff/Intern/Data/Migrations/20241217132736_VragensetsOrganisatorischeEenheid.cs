using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    /// <inheritdoc />
    public partial class VragensetsOrganisatorischeEenheid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AfdelingNaam",
                table: "ContactVerzoekVragenSets",
                newName: "OrganisatorischeEenheidNaam");

            migrationBuilder.RenameColumn(
                name: "AfdelingId",
                table: "ContactVerzoekVragenSets",
                newName: "OrganisatorischeEenheidId");

            migrationBuilder.AddColumn<string>(
                name: "OrganisatorischeEenheidSoort",
                table: "ContactVerzoekVragenSets",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganisatorischeEenheidSoort",
                table: "ContactVerzoekVragenSets");

            migrationBuilder.RenameColumn(
                name: "OrganisatorischeEenheidNaam",
                table: "ContactVerzoekVragenSets",
                newName: "AfdelingNaam");

            migrationBuilder.RenameColumn(
                name: "OrganisatorischeEenheidId",
                table: "ContactVerzoekVragenSets",
                newName: "AfdelingId");
        }
    }
}
