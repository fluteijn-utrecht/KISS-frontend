using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Berichten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateCreated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublicatieDatum = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublicatieEinddatum = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Titel = table.Column<string>(type: "text", nullable: false),
                    Inhoud = table.Column<string>(type: "text", nullable: false),
                    IsBelangrijk = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Berichten", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateCreated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Naam = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

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
                name: "IX_Berichten_Type",
                table: "Berichten",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_BerichtSkill_SkillsId",
                table: "BerichtSkill",
                column: "SkillsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BerichtSkill");

            migrationBuilder.DropTable(
                name: "Berichten");

            migrationBuilder.DropTable(
                name: "Skills");
        }
    }
}
