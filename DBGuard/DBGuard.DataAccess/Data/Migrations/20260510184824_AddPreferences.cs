using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBGuard.DataAccess.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Preferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferences", x => x.Id);
                });
            
            migrationBuilder.InsertData(
                table: "Preferences",
                columns: new[]
                {
                    "Data"
                },
                values: new object[]
                {
                    "{\"SmtpHost\":\"\",\"Port\":0,\"Username\":\"\",\"PasswordEncrypted\":\"\",\"UseTls\":true,\"FromEmail\":\"\"}"
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Preferences");
        }
    }
}
