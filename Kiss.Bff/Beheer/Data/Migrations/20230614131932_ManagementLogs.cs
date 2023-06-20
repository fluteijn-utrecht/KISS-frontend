using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiss.Bff.NieuwsEnWerkinstructies.Migrations
{
    public partial class ManagementLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactmomentManagementLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Startdatum = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Einddatum = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Resultaat = table.Column<string>(type: "text", nullable: true),
                    PrimaireVraagWeergave = table.Column<string>(type: "text", nullable: true),
                    AfwijkendOnderwerp = table.Column<string>(type: "text", nullable: true),
                    EmailadresKcm = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactmomentManagementLogs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactmomentManagementLogs");
        }
    }
}
