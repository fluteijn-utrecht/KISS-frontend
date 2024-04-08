using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    public partial class Gespreksresultaten : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gespreksresultaten",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Definitie = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gespreksresultaten", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gespreksresultaten_Definitie",
                table: "Gespreksresultaten",
                column: "Definitie",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gespreksresultaten");
        }
    }
}
