using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    public partial class RenameTablesAndProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactmomentManagementLogs");

            migrationBuilder.CreateTable(
                name: "ContactMomentDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Startdatum = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Einddatum = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Gespreksresultaat = table.Column<string>(type: "text", nullable: true),
                    Vraag = table.Column<string>(type: "text", nullable: true),
                    SpecifiekeVraag = table.Column<string>(type: "text", nullable: true),
                    EmailadresKcm = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMomentDetails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactMomentDetails_Vraag",
                table: "ContactMomentDetails",
                column: "Vraag");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactMomentDetails");

            migrationBuilder.CreateTable(
                name: "ContactmomentManagementLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AfwijkendOnderwerp = table.Column<string>(type: "text", nullable: true),
                    Einddatum = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EmailadresKcm = table.Column<string>(type: "text", nullable: true),
                    PrimaireVraagWeergave = table.Column<string>(type: "text", nullable: true),
                    Resultaat = table.Column<string>(type: "text", nullable: true),
                    Startdatum = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactmomentManagementLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactmomentManagementLogs_PrimaireVraagWeergave",
                table: "ContactmomentManagementLogs",
                column: "PrimaireVraagWeergave");
        }
    }
}
