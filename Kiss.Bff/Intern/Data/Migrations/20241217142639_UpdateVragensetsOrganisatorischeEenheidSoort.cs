using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVragensetsOrganisatorischeEenheidSoort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"ContactVerzoekVragenSets\" SET \"OrganisatorischeEenheidSoort\" = 'afdeling' Where \"OrganisatorischeEenheidSoort\" is null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
