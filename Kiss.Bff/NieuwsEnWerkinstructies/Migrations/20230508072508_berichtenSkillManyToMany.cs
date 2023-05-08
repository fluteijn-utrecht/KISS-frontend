using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    public partial class berichtenSkillManyToMany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_Berichten_BerichtId",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Skills_BerichtId",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "BerichtId",
                table: "Skills");

            migrationBuilder.CreateTable(
                name: "BerichtSkill",
                columns: table => new
                {
                    BerichtenId = table.Column<int>(type: "integer", nullable: false),
                    SkillsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BerichtSkill", x => new { x.BerichtenId, x.SkillsId });
                    table.ForeignKey(
                        name: "FK_BerichtSkill_Berichten_BerichtenId",
                        column: x => x.BerichtenId,
                        principalTable: "Berichten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BerichtSkill_Skills_SkillsId",
                        column: x => x.SkillsId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BerichtSkill_SkillsId",
                table: "BerichtSkill",
                column: "SkillsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BerichtSkill");

            migrationBuilder.AddColumn<int>(
                name: "BerichtId",
                table: "Skills",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_BerichtId",
                table: "Skills",
                column: "BerichtId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_Berichten_BerichtId",
                table: "Skills",
                column: "BerichtId",
                principalTable: "Berichten",
                principalColumn: "Id");
        }
    }
}
