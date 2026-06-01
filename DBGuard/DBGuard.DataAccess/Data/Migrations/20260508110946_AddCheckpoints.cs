using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBGuard.DataAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetectionCheckpoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    EntityValue = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LastAlertTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectionCheckpoints", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetectionCheckpoints_Type_EntityValue",
                table: "DetectionCheckpoints",
                columns: new[] { "Type", "EntityValue" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetectionCheckpoints");
        }
    }
}
